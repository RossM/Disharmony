using System.Runtime.ExceptionServices;

namespace Disharmony;

internal static class InfoOf
{
    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
    public static readonly MethodInfo MethodBase_GetMethodFromHandle1
        = SymbolExtensions.GetMethodInfo(() => MethodBase.GetMethodFromHandle(new RuntimeMethodHandle()));

    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
    public static readonly MethodInfo MethodBase_GetMethodFromHandle2
        = SymbolExtensions.GetMethodInfo(() => MethodBase.GetMethodFromHandle(new RuntimeMethodHandle(), new RuntimeTypeHandle()));

    public static readonly MethodInfo FieldInfo_GetFieldFromHandle1
        = SymbolExtensions.GetMethodInfo(() => FieldInfo.GetFieldFromHandle(new RuntimeFieldHandle()));

    public static readonly MethodInfo FieldInfo_GetFieldFromHandle2
        = SymbolExtensions.GetMethodInfo(() => FieldInfo.GetFieldFromHandle(new RuntimeFieldHandle(), new RuntimeTypeHandle()));

    public static readonly MethodInfo HarmonyInterface_ResolveTrampoline = SymbolExtensions.GetMethodInfo(() => HarmonyInterface.ResolveTrampoline);

    public static readonly MethodInfo HarmonyInterface_Transpiler = SymbolExtensions.GetMethodInfo(() => HarmonyInterface.Transpiler);

    public static readonly MethodInfo ExceptionDispatchInfo_Capture = SymbolExtensions.GetMethodInfo(() => ExceptionDispatchInfo.Capture);

    public static readonly MethodInfo RuntimeHelpers_RethrowException = SymbolExtensions.GetMethodInfo(() => RuntimeHelpers.RethrowException);

    public static readonly FieldInfo DynamicMethod_ReturnType =
        typeof(DynamicMethod).GetField("returnType", BindingFlags.NonPublic | BindingFlags.Instance) ??
        typeof(DynamicMethod).GetField("_returnType", BindingFlags.NonPublic | BindingFlags.Instance) ??
        typeof(DynamicMethod).GetField("m_returnType", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Cannot find returnType field on DynamicMethod");
}
