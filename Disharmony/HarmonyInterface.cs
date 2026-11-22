using JetBrains.Annotations;

namespace Disharmony;

internal interface IInlineSignature
{
    int ParameterCount { get; }
    bool HasImplicitThis { get; }
    Type ReturnType { get; }
}

internal abstract class HarmonyInterface : IBackend
{
    protected struct MethodPatch
    {
        public required Ruleset ruleset;
        public bool optimize;
        public bool debug;
    }

    protected class WrappedInlineSignature(object inlineSignature, HarmonyInterface harmonyInterface) : IInlineSignature
    {
        public object InlineSignature => inlineSignature;
        public int ParameterCount => harmonyInterface.InlineSignatureParameters(inlineSignature).Count;
        public bool HasImplicitThis => harmonyInterface.InlineSignatureHasThis(inlineSignature);
        public Type ReturnType => (Type)harmonyInterface.InlineSignatureReturnType(inlineSignature);
    }

    protected const string HarmonyID = "Xylthixlm.Disharmony.Autopatcher";

    private static readonly ModuleBuilder module;

    // These variables must only be accessed while Locker is held
    protected readonly Dictionary<MethodBase, MethodInfo> trampolines = [];
    private int trampolineCount;

    protected readonly Dictionary<MethodBase, MethodPatch> methodPatches = [];

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

    // Detect the backend in the assembly Disharmony binds to, even if other Harmony copies are loaded.
    internal static Type GetHarmonyType(string name) =>
        typeof(Harmony).Assembly.GetType(name) ?? throw new NotSupportedException($"Unsupported Harmony internals: {name} in {typeof(Harmony).Assembly.FullName}.");

#if DEBUG
    public abstract event Action? ApplyPatchHookForTesting;
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

    /// <summary>
    ///     Resolves deferred trampolines until the worklist is empty.
    /// </summary>
    /// <remarks>
    ///     PatchDirectly runs under Harmony's lock. Other threads may add or remove trampolines between iterations.
    ///     Returned failures are reported after releasing the lock, and processing continues. Exceptions from Harmony's
    ///     recovery path or the error handler propagate. This error-handling behavior is provisional.
    /// </remarks>
    public void ResolveAllTrampolines()
    {
        while (true)
        {
            MethodBase method;
            Exception? exception;
            lock (Locker)
            {
                if (trampolines.Count == 0)
                    return;
                method = trampolines.Keys.First();
                exception = PatchDirectly(method);
                trampolines.Remove(method);
            }

            if (exception != null)
                Patcher.ReportException(new RuntimePatchException($"Error patching {method.FullName}", exception));
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
        HarmonyInterface harmonyInterface = (HarmonyInterface)Backend.Instance;

        var instructionsList = instructions.ToList();
        var patch = harmonyInterface.methodPatches[method];

        foreach (var instruction in instructionsList)
        {
            if (instruction.operand?.GetType() == harmonyInterface.InlineSignatureType)
                instruction.operand = new WrappedInlineSignature(instruction.operand, harmonyInterface);
        }

        try
        {
            var allocator = new InstructionAllocator(generator);
            patch.ruleset.MatchAndReplace(method, ref instructionsList, allocator);
            if (instructionsList.Any(inst => inst.blocks.Count > 0))
                ExceptionFixup.Fix(method, ref instructionsList, allocator);
        }
        catch (Exception e)
        {
            Patcher.ReportException(e);
            return instructionsList;
        }

        if (Backend.optimizerEnabled && patch.optimize)
            try
            {
                var optimizer = new Optimizer.Optimizer(method, instructionsList, generator, debug: patch.debug);
                instructionsList = optimizer.Optimize();
            }
            catch (Exception e)
            {
                Patcher.ReportException(e);
            }

        foreach (var instruction in instructionsList)
        {
            if (instruction.operand is WrappedInlineSignature wrappedInlineSignature)
                instruction.operand = wrappedInlineSignature.InlineSignature;
        }

        return instructionsList;
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

    protected static TDelegate Bind<TDelegate>(string typeName, params string[] methodNames)
        where TDelegate : Delegate
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;
        Type type = GetHarmonyType(typeName);
        MethodInfo invoke = typeof(TDelegate).GetMethod("Invoke")!;
        Type[] parameterTypes = [.. invoke.GetParameters().Select(p => p.ParameterType)];

        MethodInfo? FindMethod(string name) =>
            type.GetMethod(name, flags | BindingFlags.Static, null, parameterTypes, null) ??
            type.GetMethod(name, flags | BindingFlags.Instance, null, [.. parameterTypes.Skip(1)], null);

        MethodInfo? method = null;
        foreach (string methodName in methodNames)
        {
            method = FindMethod(methodName);
            if (method is not null)
                break;
        }

        if (method is null || method.ReturnType != invoke.ReturnType)
            throw new NotSupportedException($"Unsupported Harmony internals: {typeName}.{methodNames[0]} in {typeof(Harmony).Assembly.FullName}.");

        return AccessTools.MethodDelegate<TDelegate>(method);
    }

    protected static object GetLocker()
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        Type patchProcessor = GetHarmonyType("HarmonyLib.PatchProcessor");
        FieldInfo? field = patchProcessor.GetField("locker", flags);
        object? locker = field is not null
            ? field.GetValue(null)
            : patchProcessor.GetProperty("locker", flags)?.GetValue(null);
        return locker ?? throw new NotSupportedException($"Unsupported Harmony internals: HarmonyLib.PatchProcessor.locker in {typeof(Harmony).Assembly.FullName}.");
    }

    protected static MethodInfo GetGetter(string typeName, string propertyName) =>
        GetHarmonyType(typeName).GetProperty(propertyName)?.GetGetMethod()
        ?? throw new NotSupportedException($"Unsupported Harmony internals: {typeName}.{propertyName} in {typeof(Harmony).Assembly.FullName}.");

    [UsedImplicitly]
    public static void ResolveTrampoline(MethodBase method)
    {
        ((HarmonyInterface)Backend.Instance).ResolveTrampolineImpl(method);
    }
}
