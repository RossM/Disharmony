#if WAYLITHCLR_TESTS
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
