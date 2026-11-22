using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using HarmonyLib;
using WaylithClr;

namespace Disharmony.WaylithCLR;

internal class MethodView
{
    private sealed class TrackingAllocator() : InstructionAllocator(PatchProcessor.CreateILGenerator())
    {
        private readonly List<LocalBuilder> locals = [];
        public IReadOnlyList<LocalBuilder> Locals => locals;

        public override LocalBuilder DeclareLocal(Type type, bool pinned = false)
        {
            LocalBuilder local = base.DeclareLocal(type, pinned);
            locals.Add(local);
            return local;
        }
    }

    public IReadOnlyList<(int offset, CodeInstruction instruction)> EmittedInstructions => emittedInstructions;
    public IReadOnlyList<Type> EmittedLocalTypes => emittedLocalTypes;

    // Mapping from numerical opcode values to OpCodes
    private static readonly Dictionary<ushort, OpCode> opCodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(opCode => unchecked((ushort)opCode.Value));

    private readonly byte[] localSignature;
    private readonly TrackingAllocator allocator = new();
    private readonly int originalLocalCount;
    private readonly bool initLocals;

    private readonly List<(int offset, CodeInstruction instruction)> emittedInstructions = [];
    private readonly List<Type> emittedLocalTypes = [];

    private readonly MethodBase method;

    private MethodView(MethodBase method, in MethodBodyView body)
    {
        this.method = method;

        initLocals = body.InitLocals;

        localSignature = [.. body.LocalSignature];

        // Keep the local types available to the rules and stack analysis.
        foreach (var signature in new SignatureCodec().ReadLocals(localSignature))
            allocator.DeclareLocal(signature.Resolve(method), signature.Pinned);
        originalLocalCount = allocator.Locals.Count;
    }

    public List<CodeInstruction> CodeInstructions { get; set; } = [];
    public InstructionAllocator Allocator => allocator;

    public static unsafe MethodView Create(MethodBase method, in MethodBodyView body, ReadOnlySpan<MethodBodyClause> clauses)
    {
        var view = new MethodView(method, body);

        // Mapping from the byte start position of each instruction to the resulting CodeInstruction
        var instructionStartingAt = new Dictionary<int, CodeInstruction>();

        // Mapping from the byte end position of each instruction to the resulting CodeInstruction, used for
        // recording exception block ends.
        var instructionEndingAt = new Dictionary<int, CodeInstruction>();

        // Mapping from byte start position to label, used in GetLabel()
        var labelAt = new Dictionary<int, Label>();

        // We use a BlobReader to read the IL bytes. This isn't strictly necessary, but a BlobReader is necessary
        // for the SignatureDecoder API used in SignatureCodec, so we might as well use it here too.
        fixed (byte* bytes = body.IL)
        {
            var reader = new BlobReader(bytes, body.IL.Length);
            while (reader.RemainingBytes > 0)
            {
                // Save starting byte offset
                int offset = reader.Offset;

                // Read opcode and operand
                OpCode opCode = ReadOpCode(ref reader);
                object? operand = ReadOperand(ref reader, opCode, method);

                // Normalize accesses to locals to the forms that take a LocalBuilder operand, so that types
                // are preserved. This means replacing macros such as ldloc.0 with non-macro forms.
                ushort canonical = OpCodeData.GetCanonicalOpcode(opCode);
                if (canonical is OpCodeValues.Ldloc or OpCodeValues.Ldloca or OpCodeValues.Stloc)
                {
                    int index = OpCodeData.GetIntOperand(opCode, operand);
                    if ((uint)index >= (uint)view.originalLocalCount)
                        throw new BadImageFormatException($"Invalid local #{index} at IL_{offset:X4}.");
                    operand = view.allocator.Locals[index];
                    opCode = opCodes[canonical];
                }

                // Create the instruction and add it to the mappings for label and exception block parsing later
                var instruction = new CodeInstruction(opCode, operand);
                instructionStartingAt.Add(offset, instruction);
                instructionEndingAt.Add(reader.Offset, instruction);
                view.CodeInstructions.Add(instruction);
            }
        }

        // Convert branch instructions from byte positions to labels. This could be done during the initial
        // pass but would require passing 'labels' to ReadOperand.
        foreach (var instruction in view.CodeInstructions)
        {
            switch (instruction.opcode.OperandType)
            {
                case OperandType.InlineBrTarget or OperandType.ShortInlineBrTarget:
                    instruction.operand = GetLabel((int)instruction.operand);
                    break;
                case OperandType.InlineSwitch: instruction.operand = ((int[])instruction.operand).Select(GetLabel).ToArray(); break;
            }
        }

        // Exception clauses repeat the try range for each handler. The instruction markers
        // describe one try block followed by its handlers. If two try blocks start at the same
        // instruction, mark the outer one first so the emitter opens them in nesting order.
        foreach (var group in clauses.ToArray().GroupBy(clause => (clause.TryOffset, clause.TryLength))
                     .OrderBy(group => group.Key.TryOffset).ThenByDescending(group => group.Key.TryLength))
        {
            // Record the beginning of the 'try' block
            instructionStartingAt[group.Key.TryOffset].blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock));

            // Record the end of the entire exception clause sequence
            int end = group.Max(clause => checked(clause.HandlerOffset + clause.HandlerLength));
            instructionEndingAt[end].blocks.Add(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock));

            // Record each handler block
            foreach (var clause in group.OrderBy(clause => clause.HandlerOffset))
            {
                var blocks = instructionStartingAt[clause.HandlerOffset].blocks;
                switch (clause.Flags)
                {
                    case ExceptionHandlingClauseOptions.Clause:
                        blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock,
                            (Type)MethodBodyUpdater.ResolveToken(method, clause.ClassTokenOrFilterOffset)!));
                        break;
                    case ExceptionHandlingClauseOptions.Filter:
                        instructionStartingAt[clause.ClassTokenOrFilterOffset].blocks
                            .Add(new ExceptionBlock(ExceptionBlockType.BeginExceptFilterBlock));
                        blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock));
                        break;
                    case ExceptionHandlingClauseOptions.Finally:
                        blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock));
                        break;
                    case ExceptionHandlingClauseOptions.Fault: blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFaultBlock)); break;
                    default: throw new BadImageFormatException($"Invalid exception clause flags {clause.Flags}.");
                }
            }
        }

        return view;

        // Get the label for an offset, or create one if it doesn't exist
        Label GetLabel(int offset)
        {
            if (!instructionStartingAt.TryGetValue(offset, out var instruction))
                throw new BadImageFormatException($"Invalid branch target IL_{offset:X4}.");
            if (labelAt.TryGetValue(offset, out Label label))
                return label;
            label = view.allocator.DefineLabel();
            labelAt.Add(offset, label);
            instruction.labels.Add(label);
            return label;
        }
    }

    /// <summary>
    ///     Reads the byte(s) of an opcode from the reader and returns the corresponding OpCode
    /// </summary>
    private static OpCode ReadOpCode(ref BlobReader reader)
    {
        ushort value = reader.ReadByte();
        if (value == 0xFE)
            value = (ushort)(0xFE00 | reader.ReadByte());
        if (!opCodes.TryGetValue(value, out OpCode opCode) || opCode.OpCodeType == OpCodeType.Nternal)
            throw new BadImageFormatException($"Invalid opcode 0x{value:X4}.");
        return opCode;
    }

    /// <summary>
    ///     Reads the byte(s) of an operand from the reader and returns the corresponding operand object
    /// </summary>
    private static object? ReadOperand(ref BlobReader reader, OpCode opCode, MethodBase method)
    {
        switch (opCode.OperandType)
        {
            case OperandType.InlineNone: return null;
            case OperandType.ShortInlineI: return opCode == OpCodes.Ldc_I4_S ? reader.ReadSByte() : reader.ReadByte();
            case OperandType.ShortInlineVar: return (int)reader.ReadByte();
            case OperandType.InlineVar: return (int)reader.ReadUInt16();
            case OperandType.InlineI: return reader.ReadInt32();
            case OperandType.InlineI8: return reader.ReadInt64();
            case OperandType.ShortInlineR: return reader.ReadSingle();
            case OperandType.InlineR: return reader.ReadDouble();
            case OperandType.ShortInlineBrTarget:
            {
                int delta = reader.ReadSByte();
                return checked(reader.Offset + delta);
            }
            case OperandType.InlineBrTarget:
            {
                int delta = reader.ReadInt32();
                return checked(reader.Offset + delta);
            }
            case OperandType.InlineSwitch:
            {
                int count = reader.ReadInt32();
                if (count < 0 || count > reader.RemainingBytes / sizeof(int))
                    throw new BadImageFormatException("Invalid switch table length.");
                int end = checked(reader.Offset + count * sizeof(int));
                var targets = new int[count];
                for (int i = 0; i < count; i++)
                    targets[i] = checked(end + reader.ReadInt32());
                return targets;
            }
            case OperandType.InlineSig:
                return new InlineSignature(method, (byte[])MethodBodyUpdater.ResolveToken(method, reader.ReadInt32())!);
            case OperandType.InlineField or OperandType.InlineMethod or OperandType.InlineType or OperandType.InlineTok or
                 OperandType.InlineString:
                return MethodBodyUpdater.ResolveToken(method, reader.ReadInt32());
            default: throw new BadImageFormatException($"Unsupported operand encoding {opCode.OperandType}.");
        }
    }

    public void Emit(MethodBodyBuilder builder)
    {
        var emitter = new MethodEmitter(method, builder, allocator, emittedInstructions);
        emitter.Write(CodeInstructions, WriteLocalSignature(builder), initLocals);
        emittedLocalTypes.AddRange(allocator.Locals.Select(local => local.LocalType));
    }

    private unsafe StandaloneSignatureHandle WriteLocalSignature(MethodBodyBuilder builder)
    {
        int count = allocator.Locals.Count;
        if (count == 0)
            return default;
        if (count == originalLocalCount)
            return SignatureHandle(localSignature);

        var blob = new BlobBuilder();
        var locals = new BlobEncoder(blob).LocalVariableSignature(count);
        if (localSignature.Length > 0)
            fixed (byte* bytes = localSignature)
            {
                // Keep modifiers and array bounds that are absent from the LocalBuilders.
                var reader = new BlobReader(bytes, localSignature.Length);
                reader.ReadSignatureHeader();
                reader.ReadCompressedInteger();
                blob.WriteBytes(localSignature, reader.Offset, reader.RemainingBytes);
            }

        for (int i = originalLocalCount; i < count; i++)
            LocalSignatureEncoder.Write(locals.AddVariable(), allocator.Locals[i], builder);
        return SignatureHandle(blob.ToArray());

        StandaloneSignatureHandle SignatureHandle(byte[] bytes) =>
            (StandaloneSignatureHandle)MetadataTokens.Handle(builder.GetTokenForSignature(bytes));
    }
}
