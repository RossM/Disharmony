#if WAYLITHCLR_TESTS
using System.IO;
using System.Text;
using Disharmony.RulesEngine;

namespace Disharmony.Tests.EndToEnd.Interop;

public static class MethodViewPatches
{
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static int RoundTrip_PreservesUnmanagedCalliSignature_Cdecl(int value) => value * 2;

    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    public static int RoundTrip_PreservesUnmanagedCalliSignature_StdCall(int value) => value * 2;
}

[TestFixture]
public sealed class MethodViewTests : PatchTestBase
{
    [TestCase(nameof(OpCodes.Ldc_I4), (byte)200, typeof(int), 200, 10)]
    [TestCase(nameof(OpCodes.Ldc_I4), (short)-129, typeof(int), -129, 10)]
    [TestCase(nameof(OpCodes.Ldc_I4), 1000L, typeof(int), 1000, 10)]
    [TestCase(nameof(OpCodes.Ldc_I4), BindingFlags.ExactBinding, typeof(int), 65536, 10)]
    [TestCase(nameof(OpCodes.Ldc_I4_S), (short)-12, typeof(int), -12, 7)]
    [TestCase(nameof(OpCodes.Ldc_I4_S), (byte)12, typeof(int), 12, 7)]
    [TestCase(nameof(OpCodes.Ldc_I8), 123456, typeof(long), 123456L, 14)]
    [TestCase(nameof(OpCodes.Ldc_I8), (uint)42, typeof(long), 42L, 14)]
    [TestCase(nameof(OpCodes.Ldc_R4), 1.25, typeof(float), 1.25f, 10)]
    [TestCase(nameof(OpCodes.Ldc_R8), 1.25f, typeof(double), 1.25, 14)]
    public void NumericOperands_UseOpcodeEncoding(string opcodeName, object operand, Type valueType, object expected, int returnOffset)
    {
        OpCode opcode = (OpCode)typeof(OpCodes).GetField(opcodeName)!.GetValue(null)!;
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(NumericOperands_UseOpcodeEncoding)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var observed = type.DefineField("Observed", valueType, FieldAttributes.Public | FieldAttributes.Static);
        var method = type.DefineMethod("Touch", MethodAttributes.Public | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        method.GetILGenerator().Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Touch")!;
        FieldInfo field = targetType.GetField("Observed")!;
        var rules = new Ruleset(new Rule
        {
            Mode = OutputMode.MethodPrefix,
            Output = [new CodeInstruction(opcode, operand), new CodeInstruction(OpCodes.Stsfld, field)],
        });
        FileLog.FlushBuffer();
        using var logStream = new MemoryStream();
        using var writer = new StreamWriter(logStream) { AutoFlush = true };
        var previousWriter = FileLog.LogWriter;
        FileLog.LogWriter = writer;
        try
        {
            Backend.Instance.Patch(new MethodInvocation(target), rules, false, debug: true, optimize: false);
            Backend.Instance.Apply();
            target.Invoke(null, null);
            Assert.That(field.GetValue(null), Is.EqualTo(expected));
            FileLog.FlushBuffer();
            string log = Encoding.UTF8.GetString(logStream.ToArray());
            Assert.That(log, Does.Match($@"IL_{returnOffset:X4}:\s+ret"));
        }
        finally
        {
            try
            {
                Backend.Instance.Unpatch(target);
                Backend.Instance.Apply();
            }
            finally { FileLog.LogWriter = previousWriter; }
        }
    }

    [TestCase(nameof(OpCodes.Ldarg), (byte)1, 4)]
    [TestCase(nameof(OpCodes.Ldarg), 1L, 4)]
    [TestCase(nameof(OpCodes.Ldarg), (ushort)1, 4)]
    [TestCase(nameof(OpCodes.Ldarg_S), (short)1, 2)]
    [TestCase(nameof(OpCodes.Ldarg_S), 1L, 2)]
    public void ArgumentIndexes_UseOpcodeWidth(string opcodeName, object operand, int returnOffset)
    {
        OpCode opcode = (OpCode)typeof(OpCodes).GetField(opcodeName)!.GetValue(null)!;
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(ArgumentIndexes_UseOpcodeWidth)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(int), typeof(int)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        method.GetILGenerator().Emit(OpCodes.Ldc_I4_0);
        method.GetILGenerator().Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<int, int, int>)target.CreateDelegate(typeof(Func<int, int, int>));
        var rules = new Ruleset(new Rule
        {
            Pattern = [new CodeInstruction(OpCodes.Ldc_I4_0)],
            Output = [new CodeInstruction(opcode, operand)],
        });
        FileLog.FlushBuffer();
        using var logStream = new MemoryStream();
        using var writer = new StreamWriter(logStream) { AutoFlush = true };
        var previousWriter = FileLog.LogWriter;
        FileLog.LogWriter = writer;
        try
        {
            Backend.Instance.Patch(new MethodInvocation(target), rules, false, debug: true, optimize: false);
            Backend.Instance.Apply();
            Assert.That(invoke(11, 42), Is.EqualTo(42));
            FileLog.FlushBuffer();
            string log = Encoding.UTF8.GetString(logStream.ToArray());
            Assert.That(log, Does.Match($@"IL_{returnOffset:X4}:\s+ret"));
        }
        finally
        {
            try
            {
                Backend.Instance.Unpatch(target);
                Backend.Instance.Apply();
            }
            finally { FileLog.LogWriter = previousWriter; }
        }
    }

    [TestCase((short)1)]
    [TestCase(1L)]
    public void UnalignedPrefix_UsesUnsignedByteOperand(object operand)
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(UnalignedPrefix_UsesUnsignedByteOperand)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var source = type.DefineField("Source", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        var observed = type.DefineField("Observed", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        var method = type.DefineMethod("Touch", MethodAttributes.Public | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        method.GetILGenerator().Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Touch")!;
        targetType.GetField("Source")!.SetValue(null, 42);
        var rules = new Ruleset(new Rule
        {
            Mode = OutputMode.MethodPrefix,
            Output = [
                new CodeInstruction(OpCodes.Ldsflda, targetType.GetField("Source")!),
                new CodeInstruction(OpCodes.Unaligned, operand),
                new CodeInstruction(OpCodes.Ldind_I4),
                new CodeInstruction(OpCodes.Stsfld, targetType.GetField("Observed")!),
            ],
        });
        try
        {
            Backend.Instance.Patch(new MethodInvocation(target), rules, false, debug: false, optimize: false);
            Backend.Instance.Apply();
            target.Invoke(null, null);
            Assert.That(targetType.GetField("Observed")!.GetValue(null), Is.EqualTo(42));
        }
        finally
        {
            Backend.Instance.Unpatch(target);
            Backend.Instance.Apply();
        }
    }

    [TestCase(nameof(OpCodes.Ldc_I4), 2147483648L)]
    [TestCase(nameof(OpCodes.Ldc_I4_S), 128)]
    [TestCase(nameof(OpCodes.Ldarg_S), 256)]
    [TestCase(nameof(OpCodes.Ldarg), 65536)]
    public void NumericOperands_OutOfRange_ReportOverflow(string opcodeName, object operand)
    {
        OpCode opcode = (OpCode)typeof(OpCodes).GetField(opcodeName)!.GetValue(null)!;
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(NumericOperands_OutOfRange_ReportOverflow)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(int)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        method.GetILGenerator().Emit(OpCodes.Ldc_I4_7);
        method.GetILGenerator().Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var rules = new Ruleset(new Rule
        {
            Pattern = [new CodeInstruction(OpCodes.Ldc_I4_7)],
            Output = [new CodeInstruction(opcode, operand)],
        });
        List<Exception> reports = [];
        Action<Exception> handler = reports.Add;
        Patcher.RuntimeExceptionHandler -= RecordRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            Backend.Instance.Patch(new MethodInvocation(target), rules, false, debug: false, optimize: false);
            Backend.Instance.Apply();
            Assert.That(reports, Has.Count.EqualTo(1));
            Assert.That(reports[0], Is.TypeOf<OverflowException>());
            Assert.That(target.Invoke(null, [42]), Is.EqualTo(7));
        }
        finally
        {
            try
            {
                Backend.Instance.Unpatch(target);
                Backend.Instance.Apply();
            }
            finally
            {
                Patcher.RuntimeExceptionHandler -= handler;
                Patcher.RuntimeExceptionHandler += RecordRuntimeException;
            }
        }
    }

    [TestCase(System.Runtime.InteropServices.CallingConvention.Cdecl)]
    [TestCase(System.Runtime.InteropServices.CallingConvention.StdCall)]
    public void RoundTrip_PreservesUnmanagedCalliSignature(System.Runtime.InteropServices.CallingConvention convention)
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(RoundTrip_PreservesUnmanagedCalliSignature)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(nint), typeof(int)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_0);
        il.EmitCalli(OpCodes.Calli, convention, typeof(int), [typeof(int)]);
        il.Emit(OpCodes.Ret);

        var patch = type.DefineMethod("Postfix", MethodAttributes.Public | MethodAttributes.Static, typeof(void), [typeof(int).MakeByRefType()]);
        patch.DefineParameter(1, ParameterAttributes.None, "__result");
        var patchIL = patch.GetILGenerator();
        patchIL.Emit(OpCodes.Ldarg_0);
        patchIL.Emit(OpCodes.Dup);
        patchIL.Emit(OpCodes.Ldind_I4);
        patchIL.Emit(OpCodes.Ldc_I4_1);
        patchIL.Emit(OpCodes.Add);
        patchIL.Emit(OpCodes.Stind_I4);
        patchIL.Emit(OpCodes.Ret);

        type.CreateType();
        using var stream = new System.IO.MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<nint, int, int>)target.CreateDelegate(typeof(Func<nint, int, int>));
        MethodInfo unmanaged = typeof(MethodViewPatches).GetMethod(convention == System.Runtime.InteropServices.CallingConvention.Cdecl
            ? nameof(MethodViewPatches.RoundTrip_PreservesUnmanagedCalliSignature_Cdecl)
            : nameof(MethodViewPatches.RoundTrip_PreservesUnmanagedCalliSignature_StdCall))!;
        nint pointer = unmanaged.MethodHandle.GetFunctionPointer();
        Assert.That(invoke(pointer, 6), Is.EqualTo(12));
        PatchHandle handle = Patcher.Patch(Patch.Postfix.Of(target).With(targetType.GetMethod("Postfix")!));
        try
        {
            Assert.That(invoke(pointer, 6), Is.EqualTo(13));
            Assert.That(invoke(pointer, -3), Is.EqualTo(-5));
        }
        finally
        {
            Patcher.Unpatch(handle);
        }
        Assert.That(invoke(pointer, 6), Is.EqualTo(12));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void RuleSuppliedEmptyCatch_LeavesAfterTerminatedTry(bool filtered)
    {
        Func<int> invoke = Target;
        CodeInstruction[] filter = filtered
            ? [
                new CodeInstruction(OpCodes.Pop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptFilterBlock)),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Endfilter),
            ]
            : [];
        var rules = new Ruleset(new Rule
        {
            Mode = OutputMode.MethodPrefix,
            Output =
            [
                new CodeInstruction(OpCodes.Newobj, typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!)
                    .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Throw),
                .. filter,
                // An annotation carries the handler markers without emitting an instruction.
                CodeInstruction.Annotation("Empty catch")
                    .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, filtered ? null : typeof(InvalidOperationException)),
                        new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            ],
        });

        try
        {
            Backend.Instance.Patch(new MethodInvocation(invoke.Method), rules, false, debug: false, optimize: false);
            Backend.Instance.Apply();
            Assert.That(invoke(), Is.EqualTo(42));
            var clause = invoke.Method.GetMethodBody()!.ExceptionHandlingClauses.Single();
            Assert.That(clause.Flags, Is.EqualTo(filtered ? ExceptionHandlingClauseOptions.Filter : ExceptionHandlingClauseOptions.Clause));
            Assert.That(clause.HandlerLength, Is.GreaterThan(0));
        }
        finally
        {
            Backend.Instance.Unpatch(invoke.Method);
            Backend.Instance.Apply();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static int Target() => 42;
    }

    [Test]
    public void RoundTrip_PreservesFaultHandlerOnExceptionalPathOnly()
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(RoundTrip_PreservesFaultHandlerOnExceptionalPathOnly)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var faults = type.DefineField("FaultCalls", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        var prefixes = type.DefineField("PrefixCalls", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(bool)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        LocalBuilder result = il.DeclareLocal(typeof(int));
        Label normal = il.DefineLabel();
        Label end = il.BeginExceptionBlock();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Brfalse_S, normal);
        il.Emit(OpCodes.Newobj, typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Throw);
        il.MarkLabel(normal);
        il.Emit(OpCodes.Ldc_I4_5);
        il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Leave, end);
        il.BeginFaultBlock();
        il.Emit(OpCodes.Ldsfld, faults);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stsfld, faults);
        il.EndExceptionBlock();
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ret);

        var patch = type.DefineMethod("Prefix", MethodAttributes.Public | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
        var patchIL = patch.GetILGenerator();
        patchIL.Emit(OpCodes.Ldsfld, prefixes);
        patchIL.Emit(OpCodes.Ldc_I4_1);
        patchIL.Emit(OpCodes.Add);
        patchIL.Emit(OpCodes.Stsfld, prefixes);
        patchIL.Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new System.IO.MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        FieldInfo faultCalls = targetType.GetField("FaultCalls")!;
        FieldInfo prefixCalls = targetType.GetField("PrefixCalls")!;
        var invoke = (Func<bool, int>)targetType.GetMethod("Value")!.CreateDelegate(typeof(Func<bool, int>));
        Assert.That(invoke(false), Is.EqualTo(5));
        Assert.That(faultCalls.GetValue(null), Is.EqualTo(0));
        Assert.Throws<InvalidOperationException>(() => invoke(true));
        Assert.That(faultCalls.GetValue(null), Is.EqualTo(1));
        faultCalls.SetValue(null, 0);

        PatchHandle handle = Patcher.Patch(Patch.Prefix.Of(targetType.GetMethod("Value")!).With(targetType.GetMethod("Prefix")!));
        try
        {
            Assert.That(invoke(false), Is.EqualTo(5));
            Assert.That(faultCalls.GetValue(null), Is.EqualTo(0));
            Assert.That(prefixCalls.GetValue(null), Is.EqualTo(1));
            Assert.Throws<InvalidOperationException>(() => invoke(true));
            Assert.That(faultCalls.GetValue(null), Is.EqualTo(1));
            Assert.That(prefixCalls.GetValue(null), Is.EqualTo(2));
        }
        finally
        {
            Patcher.Unpatch(handle);
        }
        Assert.That(invoke(false), Is.EqualTo(5));
        Assert.That(prefixCalls.GetValue(null), Is.EqualTo(2));
        Assert.Throws<InvalidOperationException>(() => invoke(true));
        Assert.That(faultCalls.GetValue(null), Is.EqualTo(2));
        Assert.That(prefixCalls.GetValue(null), Is.EqualTo(2));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void RoundTrip_PreservesEarlyEndfinallyInFinallyAndFaultHandlers(bool fault)
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(RoundTrip_PreservesEarlyEndfinallyInFinallyAndFaultHandlers)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var early = type.DefineField("EarlyCalls", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        var late = type.DefineField("LateCalls", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        var prefixes = type.DefineField("PrefixCalls", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(bool)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        LocalBuilder result = il.DeclareLocal(typeof(int));
        Label slow = il.DefineLabel();
        Label end = il.BeginExceptionBlock();
        if (fault)
        {
            il.Emit(OpCodes.Newobj, typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!);
            il.Emit(OpCodes.Throw);
            il.BeginFaultBlock();
        }
        else
        {
            il.Emit(OpCodes.Ldc_I4, 42);
            il.Emit(OpCodes.Stloc, result);
            il.Emit(OpCodes.Leave, end);
            il.BeginFinallyBlock();
        }
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Brfalse_S, slow);
        il.Emit(OpCodes.Ldsfld, early);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stsfld, early);
        // Both handlers can return here without executing the remaining handler instructions.
        il.Emit(OpCodes.Endfinally);
        il.MarkLabel(slow);
        il.Emit(OpCodes.Ldsfld, late);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stsfld, late);
        il.EndExceptionBlock();
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ret);

        var patch = type.DefineMethod("Prefix", MethodAttributes.Public | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
        var patchIL = patch.GetILGenerator();
        patchIL.Emit(OpCodes.Ldsfld, prefixes);
        patchIL.Emit(OpCodes.Ldc_I4_1);
        patchIL.Emit(OpCodes.Add);
        patchIL.Emit(OpCodes.Stsfld, prefixes);
        patchIL.Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new System.IO.MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        FieldInfo earlyCalls = targetType.GetField("EarlyCalls")!;
        FieldInfo lateCalls = targetType.GetField("LateCalls")!;
        FieldInfo prefixCalls = targetType.GetField("PrefixCalls")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<bool, int>)target.CreateDelegate(typeof(Func<bool, int>));
        if (fault)
            Assert.Throws<InvalidOperationException>(() => invoke(true));
        else
            Assert.That(invoke(true), Is.EqualTo(42));
        Assert.That(earlyCalls.GetValue(null), Is.EqualTo(1));
        Assert.That(lateCalls.GetValue(null), Is.EqualTo(0));
        if (fault)
            Assert.Throws<InvalidOperationException>(() => invoke(false));
        else
            Assert.That(invoke(false), Is.EqualTo(42));
        Assert.That(earlyCalls.GetValue(null), Is.EqualTo(1));
        Assert.That(lateCalls.GetValue(null), Is.EqualTo(1));
        earlyCalls.SetValue(null, 0);
        lateCalls.SetValue(null, 0);

        PatchHandle handle = Patcher.Patch(Patch.Prefix.Of(target).With(targetType.GetMethod("Prefix")!));
        try
        {
            if (fault)
                Assert.Throws<InvalidOperationException>(() => invoke(true));
            else
                Assert.That(invoke(true), Is.EqualTo(42));
            Assert.That(prefixCalls.GetValue(null), Is.EqualTo(1));
            Assert.That(earlyCalls.GetValue(null), Is.EqualTo(1));
            Assert.That(lateCalls.GetValue(null), Is.EqualTo(0));
            if (fault)
                Assert.Throws<InvalidOperationException>(() => invoke(false));
            else
                Assert.That(invoke(false), Is.EqualTo(42));
            Assert.That(prefixCalls.GetValue(null), Is.EqualTo(2));
            Assert.That(earlyCalls.GetValue(null), Is.EqualTo(1));
            Assert.That(lateCalls.GetValue(null), Is.EqualTo(1));
        }
        finally
        {
            Patcher.Unpatch(handle);
        }
    }

    [Test]
    public void RoundTrip_PreservesTypedLocalsSwitchesAndMetadataOperands()
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(RoundTrip_PreservesTypedLocalsSwitchesAndMetadataOperands)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var field = type.DefineField("Value", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(int)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        var first = il.DeclareLocal(typeof(int));
        il.DeclareLocal(typeof(List<string[]>));
        il.DeclareLocal(typeof(int[,]));
        il.DeclareLocal(typeof(byte).MakeByRefType(), true);
        LocalBuilder last = first;
        for (int i = 4; i < 260; i++)
            last = il.DeclareLocal(typeof(int));
        Label zero = il.DefineLabel();
        Label one = il.DefineLabel();
        Label finish = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Switch, new[] { zero, one });
        il.Emit(OpCodes.Ldc_I4, -129);
        il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Br_S, finish);
        il.MarkLabel(zero);
        il.Emit(OpCodes.Ldc_I4_S, (sbyte)-128);
        il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Br_S, finish);
        il.MarkLabel(one);
        il.Emit(OpCodes.Ldc_I4, 128);
        il.Emit(OpCodes.Stloc, last);
        il.Emit(OpCodes.Ldloc, last);
        il.Emit(OpCodes.Stloc_0);
        il.MarkLabel(finish);
        il.Emit(OpCodes.Ldtoken, field);
        il.Emit(OpCodes.Call, typeof(FieldInfo).GetMethod(nameof(FieldInfo.GetFieldFromHandle), [typeof(RuntimeFieldHandle)])!);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ldtoken, method);
        il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), [typeof(RuntimeMethodHandle)])!);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ldloca_S, first);
        il.Emit(OpCodes.Ldind_I4);
        il.Emit(OpCodes.Ret);

        var patch = type.DefineMethod("Postfix", MethodAttributes.Public | MethodAttributes.Static, typeof(void), [typeof(int).MakeByRefType()]);
        patch.DefineParameter(1, ParameterAttributes.None, "__result");
        var patchIL = patch.GetILGenerator();
        patchIL.Emit(OpCodes.Ldarg_0);
        patchIL.Emit(OpCodes.Dup);
        patchIL.Emit(OpCodes.Ldind_I4);
        patchIL.Emit(OpCodes.Ldc_I4_1);
        patchIL.Emit(OpCodes.Add);
        patchIL.Emit(OpCodes.Stind_I4);
        patchIL.Emit(OpCodes.Ret);

        type.CreateType();
        using var stream = new System.IO.MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<int, int>)target.CreateDelegate(typeof(Func<int, int>));
        Assert.That(invoke(0), Is.EqualTo(-128));
        var handle = Patcher.Patch(Patch.Postfix.Of(target).With(targetType.GetMethod("Postfix")!));
        try
        {
            Assert.That(invoke(0), Is.EqualTo(-127));
            Assert.That(invoke(1), Is.EqualTo(129));
            Assert.That(invoke(2), Is.EqualTo(-128));
            var locals = target.GetMethodBody()!.LocalVariables;
            Assert.That(locals[1].LocalType, Is.EqualTo(typeof(List<string[]>)));
            Assert.That(locals[2].LocalType, Is.EqualTo(typeof(int[,])));
            Assert.That(locals[3].LocalType, Is.EqualTo(typeof(byte).MakeByRefType()));
            Assert.That(locals[3].IsPinned, Is.True);
            Assert.That(locals[259].LocalType, Is.EqualTo(typeof(int)));
        }
        finally
        {
            Patcher.Unpatch(handle);
        }
        Assert.That(invoke(0), Is.EqualTo(-128));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void RoundTrip_PreservesInstanceCalliSignature(bool returnsValue, bool explicitThis)
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName($"InstanceCalliCallee_{Guid.NewGuid():N}"), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        type.DefineDefaultConstructor(MethodAttributes.Public);
        var observed = type.DefineField("Observed", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        Type returnType = returnsValue ? typeof(int) : typeof(void);
        var callee = type.DefineMethod("Callee", MethodAttributes.Public, returnType, [typeof(int)]);
        callee.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var calleeIL = callee.GetILGenerator();
        calleeIL.Emit(OpCodes.Ldarg_1);
        calleeIL.Emit(OpCodes.Ldc_I4_7);
        calleeIL.Emit(OpCodes.Add);
        calleeIL.Emit(OpCodes.Stsfld, observed);
        if (returnsValue)
            calleeIL.Emit(OpCodes.Ldsfld, observed);
        calleeIL.Emit(OpCodes.Ret);

        // Save the callee first so the explicit-this signature can reference its resolved type.
        type.CreateType();
        using var calleeStream = new MemoryStream();
        assembly.Save(calleeStream);
        calleeStream.Position = 0;
        Type calleeType = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(calleeStream).GetType("Target")!;
        assembly = new PersistedAssemblyBuilder(new AssemblyName("InstanceCalliCaller"), typeof(object).Assembly);
        type = assembly.DefineDynamicModule("Tests").DefineType("Caller", TypeAttributes.Public);

        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(int)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Newobj, calleeType.GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldftn, calleeType.GetMethod("Callee")!);
        CallingConventions convention = CallingConventions.Standard | CallingConventions.HasThis;
        if (explicitThis)
            convention |= CallingConventions.ExplicitThis;
        var callSite = SignatureHelper.GetMethodSigHelper(type.Module, convention, returnType);
        if (explicitThis)
            callSite.AddArgument(calleeType);
        callSite.AddArgument(typeof(int));
        il.Emit(OpCodes.Calli, callSite);
        if (!returnsValue)
            il.Emit(OpCodes.Ldsfld, calleeType.GetField("Observed")!);
        il.Emit(OpCodes.Ret);

        var patch = type.DefineMethod("Postfix", MethodAttributes.Public | MethodAttributes.Static, typeof(void), [typeof(int).MakeByRefType()]);
        patch.DefineParameter(1, ParameterAttributes.None, "__result");
        var patchIL = patch.GetILGenerator();
        patchIL.Emit(OpCodes.Ldarg_0);
        patchIL.Emit(OpCodes.Dup);
        patchIL.Emit(OpCodes.Ldind_I4);
        patchIL.Emit(OpCodes.Ldc_I4_1);
        patchIL.Emit(OpCodes.Add);
        patchIL.Emit(OpCodes.Stind_I4);
        patchIL.Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Caller")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<int, int>)target.CreateDelegate(typeof(Func<int, int>));
        Assert.That(invoke(5), Is.EqualTo(12));
        PatchHandle handle = Patcher.Patch(Patch.Postfix.Of(target).With(targetType.GetMethod("Postfix")!));
        try
        {
            Assert.That(invoke(5), Is.EqualTo(13));
            Assert.That(invoke(-3), Is.EqualTo(5));
        }
        finally
        {
            Patcher.Unpatch(handle);
        }
        Assert.That(invoke(5), Is.EqualTo(12));
    }

    [Test]
    public void RuleSuppliedLocal_PreservesConstructedType()
    {
        var local = PatchProcessor.CreateILGenerator().DeclareLocal(typeof(List<string[]>[,]));
        Func<List<string[]>[,], List<string[]>[,]> invoke = Target;
        var rules = new Ruleset(new Rule
        {
            Pattern = [new CodeInstruction(OpCodes.Ret)],
            Output = [new CodeInstruction(OpCodes.Stloc, local), new CodeInstruction(OpCodes.Ldloc, local),
                new CodeInstruction(OpCodes.Ret)],
        });

        try
        {
            Backend.Instance.Patch(new MethodInvocation(invoke.Method), rules, false, debug: false, optimize: false);
            Backend.Instance.Apply();
            var value = new List<string[]>[1, 1];
            Assert.That(invoke(value), Is.SameAs(value));
            Assert.That(invoke.Method.GetMethodBody()!.LocalVariables[^1].LocalType, Is.EqualTo(local.LocalType));
        }
        finally
        {
            Backend.Instance.Unpatch(invoke.Method);
            Backend.Instance.Apply();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static List<string[]>[,] Target(List<string[]>[,] value) => value;
    }

    [Test]
    public void RoundTrip_PreservesManagedCalliSignature()
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(RoundTrip_PreservesManagedCalliSignature)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(int)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldftn, typeof(Math).GetMethod(nameof(Math.Abs), [typeof(int)])!);
        il.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(int), [typeof(int)], null);
        il.Emit(OpCodes.Ret);

        var patch = type.DefineMethod("Postfix", MethodAttributes.Public | MethodAttributes.Static, typeof(void), [typeof(int).MakeByRefType()]);
        patch.DefineParameter(1, ParameterAttributes.None, "__result");
        var patchIL = patch.GetILGenerator();
        patchIL.Emit(OpCodes.Ldarg_0);
        patchIL.Emit(OpCodes.Dup);
        patchIL.Emit(OpCodes.Ldind_I4);
        patchIL.Emit(OpCodes.Ldc_I4_1);
        patchIL.Emit(OpCodes.Add);
        patchIL.Emit(OpCodes.Stind_I4);
        patchIL.Emit(OpCodes.Ret);

        type.CreateType();
        using var stream = new System.IO.MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<int, int>)target.CreateDelegate(typeof(Func<int, int>));
        Assert.That(invoke(-7), Is.EqualTo(7));
        var handle = Patcher.Patch(Patch.Postfix.Of(target).With(targetType.GetMethod("Postfix")!));
        try
        {
            Assert.That(invoke(-7), Is.EqualTo(8));
            Assert.That(invoke(3), Is.EqualTo(4));
        }
        finally
        {
            Patcher.Unpatch(handle);
        }
        Assert.That(invoke(-7), Is.EqualTo(7));
    }

    [Test]
    public void InnerPatch_WidensShortBranchAcrossInlinedPatch()
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(InnerPatch_WidensShortBranchAcrossInlinedPatch)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(int)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        Label zero = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Brfalse_S, zero);
        il.Emit(OpCodes.Ldc_I4_S, (sbyte)-128);
        il.Emit(OpCodes.Ret);
        il.MarkLabel(zero);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ret);

        var patch = type.DefineMethod("Postfix", MethodAttributes.Public | MethodAttributes.Static, typeof(void), [typeof(int).MakeByRefType()]);
        patch.DefineParameter(1, ParameterAttributes.None, "__result");
        var patchIL = patch.GetILGenerator();
        for (int i = 0; i < 200; i++)
            patchIL.Emit(OpCodes.Nop);
        patchIL.Emit(OpCodes.Ldarg_0);
        patchIL.Emit(OpCodes.Dup);
        patchIL.Emit(OpCodes.Ldind_I4);
        patchIL.Emit(OpCodes.Ldc_I4_1);
        patchIL.Emit(OpCodes.Add);
        patchIL.Emit(OpCodes.Stind_I4);
        patchIL.Emit(OpCodes.Ret);

        type.CreateType();
        using var stream = new System.IO.MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<int, int>)target.CreateDelegate(typeof(Func<int, int>));
        Assert.That(invoke(1), Is.EqualTo(-128));
        var handle = Patcher.Patch(Patch.Postfix.Of(target).With(targetType.GetMethod("Postfix")!)
            .InnerConstant(-128).Options(PatchOptions.Inline));
        try
        {
            Assert.That(invoke(1), Is.EqualTo(-127));
            Assert.That(invoke(0), Is.Zero);
        }
        finally
        {
            Patcher.Unpatch(handle);
        }
        Assert.That(invoke(1), Is.EqualTo(-128));
    }

    [Test]
    public void RoundTrip_PreservesFilterMultipleCatchesAndNestedFinally()
    {
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(RoundTrip_PreservesFilterMultipleCatchesAndNestedFinally)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(int)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        var result = il.DeclareLocal(typeof(int));
        Label zero = il.DefineLabel();
        Label one = il.DefineLabel();
        il.BeginExceptionBlock();
        Label end = il.BeginExceptionBlock();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Brfalse_S, zero);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Beq_S, one);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Leave, end);
        il.MarkLabel(zero);
        il.Emit(OpCodes.Newobj, typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Throw);
        il.MarkLabel(one);
        il.Emit(OpCodes.Newobj, typeof(ArgumentException).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Throw);
        il.BeginExceptFilterBlock();
        il.Emit(OpCodes.Isinst, typeof(InvalidOperationException));
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Cgt_Un);
        il.BeginCatchBlock(null);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ldc_I4, 11);
        il.Emit(OpCodes.Stloc, result);
        il.BeginCatchBlock(typeof(ArgumentException));
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ldc_I4, 22);
        il.Emit(OpCodes.Stloc, result);
        il.EndExceptionBlock();
        il.BeginFinallyBlock();
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ldc_I4, 10);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, result);
        il.EndExceptionBlock();
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ret);

        var patch = type.DefineMethod("Prefix", MethodAttributes.Public | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
        patch.GetILGenerator().Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new System.IO.MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<int, int>)target.CreateDelegate(typeof(Func<int, int>));
        Assert.That(invoke(0), Is.EqualTo(21));
        Assert.That(invoke(1), Is.EqualTo(32));
        Assert.That(invoke(5), Is.EqualTo(15));
        var handle = Patcher.Patch(Patch.Prefix.Of(target).With(targetType.GetMethod("Prefix")!));
        try
        {
            Assert.That(invoke(0), Is.EqualTo(21));
            Assert.That(invoke(1), Is.EqualTo(32));
            Assert.That(invoke(5), Is.EqualTo(15));
            Assert.That(target.GetMethodBody()!.ExceptionHandlingClauses, Has.Count.EqualTo(3));
        }
        finally
        {
            Patcher.Unpatch(handle);
        }
    }
}
#endif
