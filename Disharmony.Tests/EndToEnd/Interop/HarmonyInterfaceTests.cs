using Disharmony.RulesEngine;
using HarmonyPatchInfo = HarmonyLib.PatchInfo;

namespace Disharmony.Tests.EndToEnd.Interop;

public static class HarmonyInterfacePatches
{
    public static IEnumerable<CodeInstruction> PassThroughTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
}

// Reflection is confined to fixture setup and metadata observation. Exercise the actual
// patching operations without adding production methods solely for these tests.
[TestFixture]
public sealed class HarmonyInterfaceTests : PatchTestBase
{
    [TestCase(false)]
    [TestCase(true)]
    public void DeferredPatch_ReusingTrampoline_PublishesUpdatedPatchInfo(bool forceApply)
    {
        HarmonyInterface harmonyInterface = HarmonyInterface.Instance;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type implementation = harmonyInterface.GetType();
        object locker = implementation.GetProperty("Locker", flags)!.GetValue(harmonyInterface)!;
        var getPatchInfo = AccessTools.MethodDelegate<Func<MethodBase, HarmonyPatchInfo?>>(
            "HarmonyLib.HarmonySharedState:GetPatchInfo");
        MethodInfo original = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.Value))!;
        MethodInfo postfix = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.IncrementResult))!;
        List<PatchHandle> handles = [];

        try
        {
            handles.Add(Patcher.Patch(Patch.Postfix.With(postfix).Of(original)));
            int firstVersion;
            lock (locker)
                firstVersion = getPatchInfo(original)!.VersionCount;

            // Register the second patch before either invoking the target or forcing compilation.
            handles.Add(Patcher.Patch(Patch.Postfix.With(postfix).Of(original)));
            lock (locker)
            {
                HarmonyPatchInfo updated = getPatchInfo(original)!;
                Assert.That(updated.VersionCount, Is.EqualTo(firstVersion + 1),
                    "Reusing a pending trampoline must still publish the patch update.");
                Assert.That(updated.transpilers.Count(p => p.PatchMethod == InfoOf.HarmonyInterface_Transpiler), Is.EqualTo(1));
            }

            if (forceApply)
                Patcher.ForceApply();

            Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(12));
            lock (locker)
                Assert.That(getPatchInfo(original)!.VersionCount, Is.EqualTo(firstVersion + 2));
        }
        finally
        {
            foreach (PatchHandle handle in handles)
                Patcher.Unpatch(handle);
        }

        Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(10));
    }

    [Test]
    public void ApplyPatch_PreservesTrampolineWhileTranspilerIsAddedRemovedAndAddedAgain()
    {
        const string owner = "Disharmony.Tests.HarmonyInterface.Republish";
        HarmonyInterface harmonyInterface = HarmonyInterface.Instance;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type implementation = harmonyInterface.GetType();
        object locker = implementation.GetProperty("Locker", flags)!.GetValue(harmonyInterface)!;
        object internals = harmonyInterface is HarmonyInterfaceImplV2
            ? implementation.GetProperty("HarmonyInternals", flags)!.GetValue(harmonyInterface)!
            : implementation.GetField("internals", flags)!.GetValue(harmonyInterface)!;
        FieldInfo getPatchInfoField = internals.GetType().GetField("GetPatchInfo")!;
        var getPatchInfo = (Func<MethodBase, HarmonyPatchInfo?>)getPatchInfoField.GetValue(internals)!;
        var detourMethod = (Action<MethodBase, MethodBase>)internals.GetType().GetField("DetourMethod")!.GetValue(internals)!;
        var pendingTrampolines = (Dictionary<MethodBase, MethodInfo>)typeof(HarmonyInterface)
            .GetField("trampolines", flags)!.GetValue(harmonyInterface)!;
        MethodInfo original = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.Value))!;
        MethodInfo trampoline = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.Trampoline))!;
        MethodInfo transpiler = typeof(HarmonyInterfacePatches).GetMethod(nameof(HarmonyInterfacePatches.PassThroughTranspiler))!;
        var harmony = new Harmony(owner);

        lock (locker)
        {
            HarmonyPatchInfo patches = getPatchInfo(original) ?? new HarmonyPatchInfo();
            int initialVersion = patches.VersionCount;
            try
            {
                // A stand-in trampoline lets us observe the live detour without resolving it.
                detourMethod(original, trampoline);
                pendingTrampolines.Add(original, trampoline);
                // Supply staged metadata while leaving registration and publication
                // to ApplyPatch's production path.
                getPatchInfoField.SetValue(internals, new Func<MethodBase, HarmonyPatchInfo?>(
                    method => method == original ? patches : getPatchInfo(method)));
                harmonyInterface.ApplyPatch(new MethodInvocation(original), new Ruleset(), useTrampolines: true, debug: false, optimize: false);
                int expectedVersion = initialVersion + 1;

                foreach (bool addTranspiler in new[] { true, false, true })
                {
                    patches = getPatchInfo(original)!;
                    patches.transpilers = [.. patches.transpilers.Where(p => p.owner != owner && p.PatchMethod != InfoOf.HarmonyInterface_Transpiler)];
                    if (addTranspiler)
                        patches.transpilers = [.. patches.transpilers, new HarmonyLib.Patch(new HarmonyMethod(transpiler), patches.transpilers.Length, owner)];

                    harmonyInterface.ApplyPatch(new MethodInvocation(original), new Ruleset(), useTrampolines: true, debug: false, optimize: false);
                    expectedVersion++;

                    HarmonyPatchInfo published = getPatchInfo(original)!;
                    Assert.That(published.VersionCount, Is.EqualTo(expectedVersion));
                    Assert.That(published.transpilers.Count(p => p.owner == owner && p.PatchMethod == transpiler),
                        Is.EqualTo(addTranspiler ? 1 : 0));
                    if (!addTranspiler)
                        Assert.That(published.transpilers, Is.Empty);
                    Assert.That(Harmony.GetOriginalMethod(trampoline), Is.EqualTo(original));
                    Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(20), "Publishing metadata must leave the trampoline installed.");
                }

                getPatchInfoField.SetValue(internals, getPatchInfo);
                harmonyInterface.ResolveTrampolineImpl(original);
                Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(10));
            }
            finally
            {
                getPatchInfoField.SetValue(internals, getPatchInfo);
                pendingTrampolines.Remove(original);
                harmonyInterface.Unpatch(original);
                harmony.Unpatch(original, HarmonyPatchType.All, owner);
            }
        }
    }

    [Test]
    public void Instance_SelectsImplementationForLoadedHarmony()
    {
        HarmonyInterface harmonyInterface = HarmonyInterface.Instance;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type implementation = harmonyInterface.GetType();
        object locker = implementation.GetProperty("Locker", flags)!.GetValue(harmonyInterface)!;
        int major = typeof(Harmony).Assembly.GetName().Version!.Major;

        Assert.That(major, Is.AnyOf(2, 3));
        Assert.That(harmonyInterface.GetType(), Is.EqualTo(major == 2
            ? typeof(HarmonyInterfaceImplV2)
            : typeof(HarmonyInterfaceImplV3)));
        Assert.That(locker, Is.SameAs(typeof(PatchProcessor)
            .GetField("locker", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ApplyPatch_PublishesEachReplacementOnce(bool installTrampoline)
    {
        const string owner = "Disharmony.Tests.HarmonyInterface";
        HarmonyInterface harmonyInterface = HarmonyInterface.Instance;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type implementation = harmonyInterface.GetType();
        object locker = implementation.GetProperty("Locker", flags)!.GetValue(harmonyInterface)!;
        object internals = harmonyInterface is HarmonyInterfaceImplV2
            ? implementation.GetProperty("HarmonyInternals", flags)!.GetValue(harmonyInterface)!
            : implementation.GetField("internals", flags)!.GetValue(harmonyInterface)!;
        FieldInfo getPatchInfoField = internals.GetType().GetField("GetPatchInfo")!;
        var getPatchInfo = (Func<MethodBase, HarmonyPatchInfo?>)getPatchInfoField.GetValue(internals)!;
        FieldInfo updateWrapperField = internals.GetType().GetField("UpdateWrapper")!;
        var updateWrapper = (Func<MethodBase, HarmonyPatchInfo, MethodInfo>)updateWrapperField.GetValue(internals)!;
        var detourMethod = (Action<MethodBase, MethodBase>)internals.GetType().GetField("DetourMethod")!.GetValue(internals)!;
        var pendingTrampolines = (Dictionary<MethodBase, MethodInfo>)typeof(HarmonyInterface)
            .GetField("trampolines", flags)!.GetValue(harmonyInterface)!;
        MethodInfo original = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.Value))!;
        MethodInfo prefix = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.Prefix))!;
        MethodInfo trampoline = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.Trampoline))!;
        var harmony = new Harmony(owner);

        lock (locker)
        {
            HarmonyPatchInfo patches = getPatchInfo(original) ?? new HarmonyPatchInfo();
            int initialVersion = patches.VersionCount;
            patches.prefixes = [.. patches.prefixes, new HarmonyLib.Patch(new HarmonyMethod(prefix), patches.prefixes.Length, owner)];
            MethodInfo? replacement = null;

            try
            {
                getPatchInfoField.SetValue(internals, new Func<MethodBase, HarmonyPatchInfo?>(
                    method => method == original ? patches : getPatchInfo(method)));
                // Observe the replacement returned by Harmony without changing its generation or publication.
                updateWrapperField.SetValue(internals, new Func<MethodBase, HarmonyPatchInfo, MethodInfo>((method, info) =>
                {
                    MethodInfo result = updateWrapper(method, info);
                    if (method == original)
                        replacement = result;
                    return result;
                }));

                if (installTrampoline)
                {
                    detourMethod(original, trampoline);
                    pendingTrampolines.Add(original, trampoline);
                }

                harmonyInterface.ApplyPatch(new MethodInvocation(original), new Ruleset(), useTrampolines: installTrampoline, debug: false, optimize: false);
                getPatchInfoField.SetValue(internals, getPatchInfo);

                if (installTrampoline)
                {
                    Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(20));
                    Assert.That(Harmony.GetOriginalMethod(trampoline), Is.EqualTo(original));
                    patches = getPatchInfo(original)!;
                    Assert.That(patches.VersionCount, Is.EqualTo(initialVersion + 1));
                    Assert.That(patches.prefixes.Single(p => p.owner == owner).PatchMethod, Is.EqualTo(prefix));
                    harmonyInterface.ResolveTrampolineImpl(original);
                }

                Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(30));
                Assert.That(replacement, Is.Not.Null);
                Assert.That(Harmony.GetOriginalMethod(replacement!), Is.EqualTo(original));
                HarmonyPatchInfo published = getPatchInfo(original)!;
                Assert.That(published.VersionCount, Is.EqualTo(initialVersion + (installTrampoline ? 2 : 1)));
                Assert.That(published.prefixes.Single(p => p.owner == owner).PatchMethod, Is.EqualTo(prefix));
            }
            finally
            {
                getPatchInfoField.SetValue(internals, getPatchInfo);
                updateWrapperField.SetValue(internals, updateWrapper);
                pendingTrampolines.Remove(original);
                harmonyInterface.Unpatch(original);
                harmony.Unpatch(original, HarmonyPatchType.All, owner);
            }

            Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(10));
        }
    }
}
