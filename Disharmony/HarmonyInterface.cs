using JetBrains.Annotations;
using HarmonyPatchInfo = HarmonyLib.PatchInfo;

namespace Disharmony;

internal abstract class HarmonyInterface
{
    protected struct MethodPatch
    {
        public required Ruleset ruleset;
        public bool optimize;
        public bool debug;
    }

    public static Type InlineSignature => Instance.InlineSignatureType;

    protected const string HarmonyID = "Xylthixlm.Disharmony.Autopatcher";

    private static readonly ModuleBuilder module;

    public static readonly HarmonyInterface Instance = Create();

    // These variables must only be accessed while Locker is held
    protected readonly Dictionary<MethodBase, MethodInfo> trampolines = [];
    private int trampolineCount;

    protected readonly Dictionary<MethodBase, MethodPatch> methodPatches = [];

    public bool optimizerEnabled = false;

    static HarmonyInterface()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicMethods"), AssemblyBuilderAccess.Run);
        module = assembly.DefineDynamicModule("DynamicModule");
    }

    // Shared with Harmony when resolving pending trampolines.
    protected abstract object Locker { get; }

    protected abstract Type InlineSignatureType { get; }
    protected abstract List<object> InlineSignatureParameters(object signature);
    protected abstract object InlineSignatureReturnType(object signature);
    protected abstract bool InlineSignatureHasThis(object signature);

    public static List<object> InlineSignature_Parameters(object inlineSignature) =>
        Instance.InlineSignatureParameters(inlineSignature);

    public static object InlineSignature_ReturnType(object inlineSignature) =>
        Instance.InlineSignatureReturnType(inlineSignature);

    public static bool InlineSignature_HasThis(object inlineSignature) =>
        Instance.InlineSignatureHasThis(inlineSignature);

    private static HarmonyInterface Create()
    {
        Type sharedState = GetHarmonyType("HarmonyLib.HarmonySharedState");

        MethodInfo? FindUpdate(Type payloadType) => sharedState.GetMethod("UpdatePatchInfo",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding, null,
            [typeof(MethodBase), typeof(MethodInfo), payloadType], null);

        if (FindUpdate(typeof(byte[]))?.ReturnType == typeof(void))
            return new HarmonyInterfaceImplV3();
        if (FindUpdate(typeof(HarmonyPatchInfo))?.ReturnType == typeof(void))
            return new HarmonyInterfaceImplV2();

        throw Unsupported("HarmonyLib.HarmonySharedState.UpdatePatchInfo");
    }

    // Detect the backend in the assembly Disharmony binds to, even if other Harmony copies are loaded.
    protected static Type GetHarmonyType(string name) =>
        typeof(Harmony).Assembly.GetType(name) ?? throw Unsupported(name);

    protected static NotSupportedException Unsupported(string member) =>
        new($"Unsupported Harmony internals: {member} in {typeof(Harmony).Assembly.FullName}.");

#if DEBUG
    internal abstract event Action? ApplyPatchHookForTesting;
#endif

    protected abstract Exception? PatchDirectly(MethodBase original);

    public void ResolveTrampolineImpl(MethodBase method)
    {
        Exception? e;
        lock (Locker)
        {
            // If we can't remove the method, we lost a race and some other thread has
            // already replaced the trampoline
            if (!trampolines.Remove(method))
                return;

            e = PatchDirectly(method);
        }

        if (e != null)
            Patcher.ReportException(e);
    }

    [UsedImplicitly]
    public static void ResolveTrampoline(MethodBase method)
    {
        Instance.ResolveTrampolineImpl(method);
    }

    /// <summary>
    ///     Eagerly apply all patches that are currently defered using trampolines.
    /// </summary>
    /// <remarks>
    ///     This method is thread-safe, but its exact behavior when another thread
    ///     is concurrently accessing Harmony can vary. It will definitely apply
    ///     all trampolines that exist when it is called, but may or may not apply
    ///     trampolines added after that point.
    /// </remarks>
    /// <exception cref="RuntimePatchException"></exception>
    public void ResolveAllTrampolines()
    {
        while (true)
        {
            lock (Locker)
            {
                if (trampolines.Count == 0)
                    return;
                var method = trampolines.Keys.First();

                Exception? e = PatchDirectly(method);
                trampolines.Remove(method);
                if (e != null)
                    throw new RuntimePatchException($"Error patching {method.FullName}", e);
            }
        }
    }

    // This method is called from within Harmony from UpdateWrapper, which is only called while
    // we or Harmony holds the Harmony lock, so it's safe to access internal state without
    // locking.
    [UsedImplicitly]
    internal static List<CodeInstruction> Transpiler(
        MethodBase method,
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var instructionsList = instructions.ToList();
        var patch = Instance.methodPatches[method];

        try
        {
            patch.ruleset.MatchAndReplace(method, ref instructionsList, generator);
            if (instructionsList.Any(inst => inst.blocks.Count > 0))
                ExceptionFixup.Fix(method, ref instructionsList, generator);
        }
        catch (Exception e)
        {
            Patcher.ReportException(e);
            return instructionsList;
        }

        if (!Instance.optimizerEnabled || !patch.optimize)
            return instructionsList;

        try
        {
            var optimizer = new Optimizer.Optimizer(method, instructionsList, generator, debug: patch.debug);
            return optimizer.Optimize();
        }
        catch (Exception e)
        {
            Patcher.ReportException(e);
            return instructionsList;
        }
    }

    protected MethodInfo MakeTrampoline(MethodBaseInvocation target)
    {
        Type[] parameterTypes = target.ParameterTypes;

        trampolineCount++;
        var method = new DynamicMethod($"{target.FullName}_Trampoline{trampolineCount}", target.ReturnType.NoRefType,
            parameterTypes, module, true);

        // DynamicMethod throws if you try to set the return type to an IsByRef type, but the restriction is only enforced
        // in the constructor; so set the return type field directly. This must be done before getting the ILGenerator.
        if (target.ReturnType.IsByRef)
            InfoOf.DynamicMethod_ReturnType.SetValue(method, target.ReturnType);

        ILGenerator generator = method.GetILGenerator();

        EmitTrampoline(target, generator);

        return method;
    }

    private static void EmitTrampoline(MethodBaseInvocation target, ILGenerator generator)
    {
        Type[] parameterTypes = target.ParameterTypes;

        EmitLoadArguments(generator, parameterTypes);

        if (target.InstanceType is { IsGenericType: true })
        {
            generator.Emit(OpCodes.Ldtoken, target);
            generator.Emit(OpCodes.Ldtoken, target.InstanceType);
            generator.Emit(OpCodes.Call, InfoOf.MethodBase_GetMethodFromHandle2);
        }
        else
        {
            generator.Emit(OpCodes.Ldtoken, target);
            generator.Emit(OpCodes.Call, InfoOf.MethodBase_GetMethodFromHandle1);
        }

        // Call ResolveTrampoline(), which generates the real patch and applies a detour
        generator.Emit(OpCodes.Call, InfoOf.HarmonyInterface_ResolveTrampoline);

        // Do a tail call to the original method, which will actually go to the newly installed patch
        // Jmp doesn't work in the case where the target is an instance method
        generator.Emit(OpCodes.Tailcall);
        generator.Emit(OpCodes.Call, target);
        generator.Emit(OpCodes.Ret);
    }

    private static void EmitLoadArguments(ILGenerator generator, Type[] parameterTypes)
    {
        // Load all arguments onto the stack
        if (parameterTypes.Length >= 1)
            generator.Emit(OpCodes.Ldarg_0);
        if (parameterTypes.Length >= 2)
            generator.Emit(OpCodes.Ldarg_1);
        if (parameterTypes.Length >= 3)
            generator.Emit(OpCodes.Ldarg_2);
        if (parameterTypes.Length >= 4)
            generator.Emit(OpCodes.Ldarg_3);
        for (int i = 4; i < parameterTypes.Length && i < 256; i++)
            generator.Emit(OpCodes.Ldarg_S, i);
        for (int i = 256; i < parameterTypes.Length; i++)
            generator.Emit(OpCodes.Ldarg, i);
    }

    public abstract void ApplyPatch(MethodBaseInvocation original, Ruleset ruleset, bool useTrampolines, bool debug, bool optimize);

    public abstract void Unpatch(MethodBase methodBase);

    protected static TDelegate Bind<TDelegate>(string typeName, string methodName)
        where TDelegate : Delegate
    {
        Type type = GetHarmonyType(typeName);
        MethodInfo invoke = typeof(TDelegate).GetMethod("Invoke")!;
        MethodInfo? method = 
            type.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding | BindingFlags.Static,
                null,
                [.. invoke.GetParameters().Select(p => p.ParameterType)],
                null) ??
            type.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding | BindingFlags.Instance,
                null,
                [.. invoke.GetParameters().Skip(1).Select(p => p.ParameterType)],
                null);
        if (method is null || method.ReturnType != invoke.ReturnType)
            throw Unsupported($"{typeName}.{methodName}");

        return AccessTools.MethodDelegate<TDelegate>(method);
    }

    protected static object GetLocker() => GetHarmonyType("HarmonyLib.PatchProcessor")
                                               .GetField("locker", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)
                                           ?? throw Unsupported("HarmonyLib.PatchProcessor.locker");

    protected static MethodInfo GetGetter(string typeName, string propertyName) =>
        GetHarmonyType(typeName).GetProperty(propertyName)?.GetGetMethod()
        ?? throw Unsupported($"{typeName}.{propertyName}");
}
