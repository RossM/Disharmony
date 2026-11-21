using HarmonyLib;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using WaylithClr;

namespace Disharmony.WaylithCLR;

internal class MethodView
{
    public List<CodeInstruction> CodeInstructions { get; set; } = [];
    public ILGenerator Generator { get; }

    private readonly TypeBuilder typeBuilder;
    private readonly MethodBuilder methodBuilder;
    private readonly byte[] localSignature;
    private readonly LocalBuilder[] originalLocals;

    // A mapping from the byte position of a Calli argument to the relevant InlineSignature
    private readonly Dictionary<int, InlineSignature> callSites = [];

    // Mapping from numerical opcode values to OpCodes
    private static readonly Dictionary<ushort, OpCode> opCodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(opCode => unchecked((ushort)opCode.Value));

    private MethodView(MethodBase method, in MethodBodyView body)
    {
        // In order to use System.Reflection.Emit to handle the work of mapping IL to bytes, we need a MethodBuilder. Define it
        // inside a DynamicAssembly so it can be garbage collected after we're done.
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Disharmony.MethodView"), AssemblyBuilderAccess.RunAndCollect);
        typeBuilder = assembly.DefineDynamicModule("Methods").DefineType("Replacement");
        Type[] parameters = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        if (!method.IsStatic)
            parameters = [method.DeclaringType!.CallableType, .. parameters];
        methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Static,
            method is MethodInfo info ? info.ReturnType : typeof(void), parameters);

        // Only have the CLR initialize the locals in the copied method if the original method did. This is a change from Harmony,
        // which always generated methods with InitLocals = true.
        methodBuilder.InitLocals = body.InitLocals;

        // Use the ILGenerator from our method body for declaring locals and labels
        Generator = methodBuilder.GetILGenerator();

        // Save the original local signature. The parsed LocalBuilders lose information from the original signature, so we
        // reuse the original signature verbatim for locals that come from the original method, and append the new locals.
        localSignature = [.. body.LocalSignature];

        // Parse the local signature to recover the types of all locals and declare them a LocalBuilders. This allows the
        // main Disharmony code to know the types of all locals (except totally unused locals, which don't matter).
        var signatureCodec = new SignatureCodec();
        originalLocals =
        [
            .. signatureCodec.ReadLocals(localSignature).Select(signature => Generator.DeclareLocal(signature.Resolve(method), signature.Pinned)),
        ];
    }

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
                    if ((uint)index >= (uint)view.originalLocals.Length)
                        throw new BadImageFormatException($"Invalid local #{index} at IL_{offset:X4}.");
                    operand = view.originalLocals[index];
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
                case OperandType.InlineSwitch:
                    instruction.operand = ((int[])instruction.operand).Select(GetLabel).ToArray();
                    break;
            }
        }
        
        // Construct the exception blocks. The incoming exception information has one entry per handler, with 'try' blocks that
        // have multiple 'catch' and/or 'finally' blocks appearing multiple times. ILGenerator expects only a single exception
        // block with multiple handlers, so we have to consolidate the 'try's. The CLR guarantees that the exception blocks
        // nest properly, so the sort here puts them in proper nesting order.
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
                        instructionStartingAt[clause.ClassTokenOrFilterOffset].blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptFilterBlock));
                        blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock));
                        break;
                    case ExceptionHandlingClauseOptions.Finally:
                        blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock));
                        break;
                    case ExceptionHandlingClauseOptions.Fault:
                        blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFaultBlock));
                        break;
                    default:
                        throw new BadImageFormatException($"Invalid exception clause flags {clause.Flags}.");
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
            label = view.Generator.DefineLabel();
            labelAt.Add(offset, label);
            instruction.labels.Add(label);
            return label;
        }
    }

    /// <summary>
    /// Reads the byte(s) of an opcode from the reader and returns the corresponding OpCode
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
    /// Reads the byte(s) of an operand from the reader and returns the corresponding operand object
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
            case OperandType.InlineField or OperandType.InlineMethod or OperandType.InlineType or OperandType.InlineTok or OperandType.InlineString:
                return MethodBodyUpdater.ResolveToken(method, reader.ReadInt32());
            default:
                throw new BadImageFormatException($"Unsupported operand encoding {opCode.OperandType}.");
        }
    }

    public unsafe void Emit(MethodBodyBuilder builder)
    {
        EmitInstructions();

        // Get the emitted IL bytes
        MethodInfo emittedMethod = typeBuilder.CreateType()!.GetMethod(methodBuilder.Name)!;
        MethodBody body = emittedMethod.GetMethodBody()!;
        byte[] il = body.GetILAsByteArray()!;

        int ImportType(int token) => builder.GetTokenFor(emittedMethod.Module.ResolveType(token));
        
        // Remap the emitted tokens to those expected by MethodBodyBuilder.
        fixed (byte* bytes = il)
        {
            var reader = new BlobReader(bytes, il.Length);
            while (reader.RemainingBytes > 0)
            {
                OpCode opCode = ReadOpCode(ref reader);
                int operandOffset = reader.Offset;
                object? operand = ReadOperand(ref reader, opCode, emittedMethod);
                int token;
                switch (operand)
                {
                    case MethodBase target: token = builder.GetTokenFor(target); break;
                    case FieldInfo field: token = builder.GetTokenFor(field); break;
                    case Type type: token = builder.GetTokenFor(type); break;
                    case string text: token = builder.GetTokenFor(text); break;
                    case InlineSignature signature:
                        // EmitCalli computes maxstack from the resolved parameter and return types.
                        // Its public overloads cannot preserve custom modifiers, so import the saved
                        // call-site signature to retain them.
                        if (callSites.TryGetValue(operandOffset, out var original))
                            signature = original;
                        token = builder.GetTokenForSignature(signature.Import(builder));
                        break;
                    default: continue;
                }
                BinaryPrimitives.WriteInt32LittleEndian(il.AsSpan(operandOffset, sizeof(int)), token);
            }
        }

        // We don't track the emitted locals explicitly so we have to parse the locals of the emitted method
        var emittedLocals = body.LocalSignatureMetadataToken == 0 ? [] :
            new SignatureCodec(ImportType).ReadLocals(emittedMethod.Module.ResolveSignature(body.LocalSignatureMetadataToken));

        // Magic number ???
        var locals = new BlobBuilder();
        locals.WriteByte(0x07);
        locals.WriteCompressedInteger(emittedLocals.Length);

        // DeclareLocal retains the Type and pinned flag, but its signature omits custom
        // modifiers and explicit array bounds. Copy the incoming local entries verbatim
        // and append the signatures of locals added during rewriting.
        if (localSignature.Length > 0)
        {
            fixed (byte* bytes = localSignature)
            {
                var reader = new BlobReader(bytes, localSignature.Length);
                reader.ReadSignatureHeader();
                reader.ReadCompressedInteger();
                locals.WriteBytes(localSignature, reader.Offset, reader.RemainingBytes);
            }
        }
        foreach (var local in emittedLocals.Skip(originalLocals.Length))
            local.WriteTo(locals);

        // Actually emit
        var output = new BlobBuilder();
        var encoder = new MethodBodyStreamEncoder(output);
        var encodedBody = encoder.AddMethodBody(il.Length, body.MaxStackSize, body.ExceptionHandlingClauses.Count,
            hasSmallExceptionRegions: false,
            localVariablesSignature: emittedLocals.Length == 0 ? default : (StandaloneSignatureHandle)MetadataTokens.Handle(builder.GetTokenForSignature(locals.ToArray())),
            attributes: body.InitLocals ? MethodBodyAttributes.InitLocals : MethodBodyAttributes.None);
        il.CopyTo(encodedBody.Instructions.GetBytes().AsSpan());
        foreach (var clause in body.ExceptionHandlingClauses)
            encodedBody.ExceptionRegions.Add((ExceptionRegionKind)clause.Flags, clause.TryOffset, clause.TryLength,
                clause.HandlerOffset, clause.HandlerLength,
                clause.Flags == ExceptionHandlingClauseOptions.Clause ? MetadataTokens.EntityHandle(builder.GetTokenFor(clause.CatchType!)) : default,
                clause.Flags == ExceptionHandlingClauseOptions.Filter ? clause.FilterOffset : 0);
        builder.Write(output.ToArray());
    }

    private void EmitInstructions()
    {
        foreach (var instruction in CodeInstructions)
        {
            foreach (Label label in instruction.labels)
                Generator.MarkLabel(label);
            foreach (var block in instruction.blocks)
            {
                switch (block.blockType)
                {
                    case ExceptionBlockType.BeginExceptionBlock: Generator.BeginExceptionBlock(); break;
                    case ExceptionBlockType.BeginCatchBlock: Generator.BeginCatchBlock(block.catchType); break;
                    case ExceptionBlockType.BeginExceptFilterBlock: Generator.BeginExceptFilterBlock(); break;
                    case ExceptionBlockType.BeginFinallyBlock: Generator.BeginFinallyBlock(); break;
                    case ExceptionBlockType.BeginFaultBlock: Generator.BeginFaultBlock(); break;
                }
            }
            EmitInstruction(instruction);

            // I do not know why Harmony chose to do this this way but this is how Harmony does it
            foreach (var block in instruction.blocks)
                if (block.blockType == ExceptionBlockType.EndExceptionBlock)
                    Generator.EndExceptionBlock();
        }
    }

    private void EmitInstruction(CodeInstruction instruction)
    {
        OpCode opCode = instruction.opcode;
        if (opCode.OperandType == OperandType.ShortInlineBrTarget)
            opCode = opCodes[OpCodeData.GetCanonicalOpcode(opCode)];
        // BeginCatchBlock(null) emits endfilter, and EndExceptionBlock emits endfinally
        // for finally and fault handlers. Emitting the parsed terminator too would leave
        // an extra instruction between the filter and its handler or after the handler.
        if (opCode == OpCodes.Endfilter || opCode == OpCodes.Endfinally)
            return;
        if (opCode.OperandType == OperandType.InlineNone)
        {
            if (opCode == OpCodes.Nop && instruction.operand is string)
                return;
            Generator.Emit(opCode);
            return;
        }
        switch (instruction.operand)
        {
            case LocalBuilder local: Generator.Emit(opCode, local); break;
            case Label label: Generator.Emit(opCode, label); break;
            case Label[] labels: Generator.Emit(opCode, labels); break;
            case MethodInfo method: Generator.Emit(opCode, method); break;
            case ConstructorInfo constructor: Generator.Emit(opCode, constructor); break;
            case FieldInfo field: Generator.Emit(opCode, field); break;
            case Type type: Generator.Emit(opCode, type); break;
            case string text: Generator.Emit(opCode, text); break;
            case InlineSignature signature:
                signature.Emit(Generator);
                callSites.Add(Generator.ILOffset - sizeof(int), signature);
                break;
            case SignatureHelper signature: Generator.Emit(opCode, signature); break;
            case float value: Generator.Emit(opCode, value); break;
            case double value: Generator.Emit(opCode, value); break;
            case long value: Generator.Emit(opCode, value); break;
            case sbyte value: Generator.Emit(opCode, value); break;
            case byte value: Generator.Emit(opCode, value); break;
            case short value: Generator.Emit(opCode, value); break;
            case int value when opCode == OpCodes.Ldc_I4_S: Generator.Emit(opCode, checked((sbyte)value)); break;
            case int value when opCode.OperandType == OperandType.ShortInlineI: Generator.Emit(opCode, checked((byte)value)); break;
            case int value when opCode.OperandType == OperandType.ShortInlineVar: Generator.Emit(opCode, checked((byte)value)); break;
            case int value when opCode.OperandType == OperandType.InlineVar: Generator.Emit(opCode, unchecked((short)checked((ushort)value))); break;
            case int value: Generator.Emit(opCode, value); break;
            default: throw new NotSupportedException($"Unsupported operand in {instruction}.");
        }
    }

    private sealed class InlineSignature(MethodBase method, byte[] bytes) : IInlineSignature
    {
        private readonly MethodSignature<SignatureCodec.TypeSignature> signature = new SignatureCodec().ReadMethod(bytes);

        public int ParameterCount => signature.ParameterTypes.Length;
        public bool HasThis => signature.Header.IsInstance;
        public Type ReturnType => signature.ReturnType.Resolve(method);

        public byte[] Import(MethodBodyBuilder builder)
        {
            int ImportType(int token) => builder.GetTokenFor((Type)MethodBodyUpdater.ResolveToken(method, token)!);
            return SignatureCodec.WriteMethod(new SignatureCodec(ImportType).ReadMethod(bytes));
        }

        public void Emit(ILGenerator generator)
        {
            Type[] parameters = signature.ParameterTypes.Select(type => type.Resolve(method)).ToArray();
            if (signature.Header.CallingConvention is SignatureCallingConvention.Default or SignatureCallingConvention.VarArgs)
            {
                CallingConventions convention = signature.Header.CallingConvention == SignatureCallingConvention.VarArgs
                    ? CallingConventions.VarArgs : CallingConventions.Standard;
                if (HasThis)
                    convention |= CallingConventions.HasThis;
                if (signature.Header.HasExplicitThis)
                    convention |= CallingConventions.ExplicitThis;
                generator.EmitCalli(OpCodes.Calli, convention, ReturnType,
                    parameters[..signature.RequiredParameterCount],
                    signature.RequiredParameterCount < parameters.Length ? parameters[signature.RequiredParameterCount..] : null);
            }
            else
            {
                CallingConvention convention = signature.Header.CallingConvention switch
                {
                    SignatureCallingConvention.CDecl => CallingConvention.Cdecl,
                    SignatureCallingConvention.StdCall => CallingConvention.StdCall,
                    SignatureCallingConvention.ThisCall => CallingConvention.ThisCall,
                    SignatureCallingConvention.FastCall => CallingConvention.FastCall,
                    SignatureCallingConvention.Unmanaged => CallingConvention.Winapi,
                    _ => throw new NotSupportedException($"Unsupported calli convention {signature.Header.CallingConvention}.")
                };
                generator.EmitCalli(OpCodes.Calli, convention, ReturnType, parameters);
            }
        }
    }
}
