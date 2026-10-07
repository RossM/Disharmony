using HarmonyPatchInfo = HarmonyLib.PatchInfo;

namespace Disharmony;

internal sealed class HarmonyInterfaceImplV2 : HarmonyInterface
{
    private sealed class HarmonyInternalsContainer
    {
        // ReSharper disable InconsistentNaming
        public readonly object locker = GetLocker();

        public readonly Func<MethodBase, HarmonyPatchInfo> GetPatchInfo
            = Bind<Func<MethodBase, HarmonyPatchInfo>>("HarmonyLib.HarmonySharedState", "GetPatchInfo");

        public readonly Action<MethodBase, MethodBase> DetourMethod
            = Bind<Action<MethodBase, MethodBase>>("HarmonyLib.PatchTools", "DetourMethod");

        public readonly Action<MethodBase, MethodInfo, HarmonyPatchInfo> UpdatePatchInfo
            = Bind<Action<MethodBase, MethodInfo, HarmonyPatchInfo>>(
                "HarmonyLib.HarmonySharedState", "UpdatePatchInfo");

        public readonly Func<MethodBase, HarmonyPatchInfo, MethodInfo> UpdateWrapper
            = Bind<Func<MethodBase, HarmonyPatchInfo, MethodInfo>>("HarmonyLib.PatchFunctions", "UpdateWrapper");

        public readonly MethodInfo InlineSignature_ReturnType_Getter = GetGetter("HarmonyLib.InlineSignature", "ReturnType");
        public readonly MethodInfo InlineSignature_Parameters_Getter = GetGetter("HarmonyLib.InlineSignature", "Parameters");
        public readonly MethodInfo InlineSignature_HasThis_Getter = GetGetter("HarmonyLib.InlineSignature", "HasThis");
        public readonly Type InlineSignature_Type = GetHarmonyType("HarmonyLib.InlineSignature")!;
        // ReSharper restore InconsistentNaming
    }

    private HarmonyInternalsContainer HarmonyInternals { get; } = new();

    protected override object Locker => HarmonyInternals.locker;

    protected override Type InlineSignatureType => HarmonyInternals.InlineSignature_Type;
    protected override List<object> InlineSignatureParameters(object signature) =>
        (List<object>)HarmonyInternals.InlineSignature_Parameters_Getter.Invoke(signature, [])!;
    protected override object InlineSignatureReturnType(object signature) =>
        (object)HarmonyInternals.InlineSignature_ReturnType_Getter.Invoke(signature, [])!;
    protected override bool InlineSignatureHasThis(object signature) =>
        (bool)HarmonyInternals.InlineSignature_HasThis_Getter.Invoke(signature, [])!;

#if DEBUG
    internal override event Action? ApplyPatchHookForTesting = null;
#endif

    /// <summary>
    ///     This does the same thing as <see cref="Harmony.Patch" />> but must be called
    ///     while we are already holding <see cref="Locker" />.
    /// </summary>
    /// <param name="original"></param>
    protected override Exception? PatchDirectly(MethodBase original)
    {
        HarmonyPatchInfo patchInfo = HarmonyInternals.GetPatchInfo(original) ?? new HarmonyPatchInfo();

        MethodInfo replacement;
        try
        {
            replacement = HarmonyInternals.UpdateWrapper(original, patchInfo);
#if ENABLE_DISASSEMBLY
            if (patchInfo.transpilers.Any(p => p.debug && p.owner == HarmonyID))
                JitAssemblyLogger.TryLog(original, replacement);
#endif
        }
        catch (Exception e)
        {
            patchInfo.transpilers =
            [
                .. patchInfo.transpilers.Where(t => t.owner != HarmonyID),
            ];

            replacement = HarmonyInternals.UpdateWrapper(original, patchInfo);

            HarmonyInternals.UpdatePatchInfo(original, replacement, patchInfo);
            return e;
        }

        HarmonyInternals.UpdatePatchInfo(original, replacement, patchInfo);
        return null;
    }

    // Must hold Locker
    private MethodInfo ApplyTrampoline(MethodBaseInvocation method)
    {
        if (trampolines.TryGetValue(method.MethodBase, out var existingTrampoline))
            return existingTrampoline;

        MethodInfo trampoline = MakeTrampoline(method);

        HarmonyInternals.DetourMethod(method.MethodBase, trampoline);

        trampolines[method.MethodBase] = trampoline;

        return trampoline;
    }

    public override void ApplyPatch(MethodBaseInvocation original, Ruleset ruleset, bool useTrampolines, bool debug, bool optimize)
    {
#if DEBUG
        ApplyPatchHookForTesting?.Invoke();
#endif

        lock (Locker)
        {
            HarmonyPatchInfo patchInfo = HarmonyInternals.GetPatchInfo(original.MethodBase) ?? new HarmonyPatchInfo();

            if (!methodPatches.ContainsKey(original.MethodBase))
            {
                HarmonyMethod patcher = new(InfoOf.HarmonyInterface_Transpiler, priority: Priority.LowerThanNormal - 1) { debug = debug };

                patchInfo.transpilers =
                [
                    .. patchInfo.transpilers,
                    new HarmonyLib.Patch(patcher, patchInfo.transpilers.Length, HarmonyID),
                ];
            }

            methodPatches[original.MethodBase] = new()
            {
                ruleset = ruleset,
                optimize = optimize,
                debug = debug,
            };

            MethodInfo replacement;
            if (useTrampolines)
                replacement = ApplyTrampoline(original);
            else
                try
                {
                    replacement = HarmonyInternals.UpdateWrapper(original.MethodBase, patchInfo);
#if ENABLE_DISASSEMBLY
                    if (patchInfo.transpilers.Any(p => p.debug && p.owner == HarmonyID))
                        JitAssemblyLogger.TryLog(original.MethodBase, replacement);
#endif
                }
                catch (Exception e)
                {
                    throw new RuntimePatchException($"Error patching {original.FullName}", e);
                }

            HarmonyInternals.UpdatePatchInfo(original.MethodBase, replacement, patchInfo);
        }
    }

    public override void Unpatch(MethodBase methodBase)
    {
        lock (Locker)
        {
            if (!methodPatches.Remove(methodBase))
                return;

            trampolines.Remove(methodBase);

            HarmonyPatchInfo patchInfo = HarmonyInternals.GetPatchInfo(methodBase) ?? new HarmonyPatchInfo();

            patchInfo.transpilers =
            [
                .. patchInfo.transpilers.Where(t => t.owner != HarmonyID),
            ];

            MethodInfo replacement = HarmonyInternals.UpdateWrapper(methodBase, patchInfo);

            HarmonyInternals.UpdatePatchInfo(methodBase, replacement, patchInfo);
        }
    }
}
