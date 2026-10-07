namespace Disharmony.Tests.EndToEnd.Interop;

public static class HarmonyCoexistencePatches
{
    public static int ObservedResult;
    public static int ObservedArgument;

    public static void HarmonyFirst_PrefixPostfix_NestAroundDisharmony_HarmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-prefix");

    public static void HarmonyFirst_PrefixPostfix_NestAroundDisharmony_HarmonyPostfix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-postfix");

    public static void HarmonyFirst_PrefixPostfix_NestAroundDisharmony_DisharmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("disharmony-prefix");

    public static void HarmonyFirst_PrefixPostfix_NestAroundDisharmony_DisharmonyPostfix() =>
        HarmonyCoexistenceTargets.Events.Add("disharmony-postfix");

    public static void DisharmonyFirst_PrefixPostfix_NestInsideHarmony_HarmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-prefix");

    public static void DisharmonyFirst_PrefixPostfix_NestInsideHarmony_HarmonyPostfix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-postfix");

    public static void DisharmonyFirst_PrefixPostfix_NestInsideHarmony_DisharmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("disharmony-prefix");

    public static void DisharmonyFirst_PrefixPostfix_NestInsideHarmony_DisharmonyPostfix() =>
        HarmonyCoexistenceTargets.Events.Add("disharmony-postfix");

    public static void UnpatchDisharmony_PreservesHarmonyPatch_HarmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-prefix");

    public static void UnpatchDisharmony_PreservesHarmonyPatch_HarmonyPostfix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-postfix");

    public static void UnpatchDisharmony_PreservesHarmonyPatch_DisharmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("disharmony-prefix");

    public static void UnpatchHarmony_PreservesDisharmonyPatch_HarmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-prefix");

    public static void UnpatchHarmony_PreservesDisharmonyPatch_HarmonyPostfix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-postfix");

    public static void UnpatchHarmony_PreservesDisharmonyPatch_DisharmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("disharmony-prefix");

    public static void UnpatchHarmony_PreservesDisharmonyPatch_DisharmonyPostfix() =>
        HarmonyCoexistenceTargets.Events.Add("disharmony-postfix");

    public static void ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch_HarmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-prefix");

    public static void ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch_HarmonyPostfix() =>
        HarmonyCoexistenceTargets.Events.Add("harmony-postfix");

    public static void ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch_FirstDisharmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("first-disharmony-prefix");

    public static void ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch_SecondDisharmonyPrefix() =>
        HarmonyCoexistenceTargets.Events.Add("second-disharmony-prefix");

    public static IEnumerable<CodeInstruction> HarmonyTranspiler_DisharmonyOuterPostfix_ObservesTranspiledResult_HarmonyTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo original = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.OriginalValue))!;
        MethodInfo replacement = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.ReplacementValue))!;

        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(original))
                instruction.operand = replacement;
            yield return instruction;
        }
    }

    public static void HarmonyTranspiler_DisharmonyOuterPostfix_ObservesTranspiledResult_DisharmonyPostfix(
        int __result) => ObservedResult = __result;

    public static IEnumerable<CodeInstruction> HarmonyTranspiler_DisharmonyInnerPrefix_ObservesTranspiledArgument_HarmonyTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo original = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.OriginalValue))!;
        MethodInfo replacement = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.ReplacementValue))!;

        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(original))
                instruction.operand = replacement;
            yield return instruction;
        }
    }

    public static void HarmonyTranspiler_DisharmonyInnerPrefix_ObservesTranspiledArgument_DisharmonyPrefix(
        int value) => ObservedArgument = value;
}

[TestFixture]
public sealed class HarmonyCoexistenceTests : PatchTestBase
{
    private const string HarmonyId = "Disharmony.Tests.HarmonyCoexistence";
    private readonly Harmony harmony = new(HarmonyId);

    [SetUp]
    public void RemoveHarmonyPatchesBeforeTest()
    {
        harmony.UnpatchAll(HarmonyId);
        HarmonyCoexistenceTargets.Events.Clear();
    }

    [TearDown]
    public void RemoveCoexistingPatchesAfterTest()
    {
        PatchRegistry.Instance.UnpatchAll();
        harmony.UnpatchAll(HarmonyId);
    }

    [Test]
    public void HarmonyFirst_PrefixPostfix_NestAroundDisharmony()
    {
        MethodInfo target = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.OrderedTarget))!;
        MethodInfo harmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.HarmonyFirst_PrefixPostfix_NestAroundDisharmony_HarmonyPrefix))!;
        MethodInfo harmonyPostfix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.HarmonyFirst_PrefixPostfix_NestAroundDisharmony_HarmonyPostfix))!;
        MethodInfo disharmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.HarmonyFirst_PrefixPostfix_NestAroundDisharmony_DisharmonyPrefix))!;
        MethodInfo disharmonyPostfix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.HarmonyFirst_PrefixPostfix_NestAroundDisharmony_DisharmonyPostfix))!;
        harmony.Patch(target, prefix: new HarmonyMethod(harmonyPrefix), postfix: new HarmonyMethod(harmonyPostfix));
        Patcher.Patch(
            Patch.Prefix.With(disharmonyPrefix).Of(target),
            Patch.Postfix.With(disharmonyPostfix).Of(target));

        int result = HarmonyCoexistenceTargets.OrderedTarget();

        Assert.That(result, Is.EqualTo(10));
        Assert.That(HarmonyCoexistenceTargets.Events, Is.EqualTo(new[]
        {
            "harmony-prefix",
            "disharmony-prefix",
            "target",
            "disharmony-postfix",
            "harmony-postfix",
        }));
    }

    [Test]
    public void DisharmonyFirst_PrefixPostfix_NestInsideHarmony()
    {
        MethodInfo target = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.OrderedTarget))!;
        MethodInfo harmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.DisharmonyFirst_PrefixPostfix_NestInsideHarmony_HarmonyPrefix))!;
        MethodInfo harmonyPostfix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.DisharmonyFirst_PrefixPostfix_NestInsideHarmony_HarmonyPostfix))!;
        MethodInfo disharmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.DisharmonyFirst_PrefixPostfix_NestInsideHarmony_DisharmonyPrefix))!;
        MethodInfo disharmonyPostfix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.DisharmonyFirst_PrefixPostfix_NestInsideHarmony_DisharmonyPostfix))!;
        Patcher.Patch(
            Patch.Prefix.With(disharmonyPrefix).Of(target),
            Patch.Postfix.With(disharmonyPostfix).Of(target));
        harmony.Patch(target, prefix: new HarmonyMethod(harmonyPrefix), postfix: new HarmonyMethod(harmonyPostfix));

        int result = HarmonyCoexistenceTargets.OrderedTarget();

        Assert.That(result, Is.EqualTo(10));
        Assert.That(HarmonyCoexistenceTargets.Events, Is.EqualTo(new[]
        {
            "harmony-prefix",
            "disharmony-prefix",
            "target",
            "disharmony-postfix",
            "harmony-postfix",
        }));
    }

    [Test]
    public void UnpatchDisharmony_PreservesHarmonyPatch()
    {
        MethodInfo target = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.OrderedTarget))!;
        MethodInfo harmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.UnpatchDisharmony_PreservesHarmonyPatch_HarmonyPrefix))!;
        MethodInfo harmonyPostfix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.UnpatchDisharmony_PreservesHarmonyPatch_HarmonyPostfix))!;
        MethodInfo disharmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.UnpatchDisharmony_PreservesHarmonyPatch_DisharmonyPrefix))!;
        harmony.Patch(target, prefix: new HarmonyMethod(harmonyPrefix), postfix: new HarmonyMethod(harmonyPostfix));
        PatchHandle handle = Patcher.Patch(Patch.Prefix.With(disharmonyPrefix).Of(target));
        HarmonyCoexistenceTargets.OrderedTarget();
        HarmonyCoexistenceTargets.Events.Clear();

        Patcher.Unpatch(handle);
        int result = HarmonyCoexistenceTargets.OrderedTarget();

        Assert.That(result, Is.EqualTo(10));
        Assert.That(HarmonyCoexistenceTargets.Events, Is.EqualTo(new[]
        {
            "harmony-prefix",
            "target",
            "harmony-postfix",
        }));
    }

    [Test]
    public void UnpatchHarmony_PreservesDisharmonyPatch()
    {
        MethodInfo target = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.OrderedTarget))!;
        MethodInfo harmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.UnpatchHarmony_PreservesDisharmonyPatch_HarmonyPrefix))!;
        MethodInfo harmonyPostfix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.UnpatchHarmony_PreservesDisharmonyPatch_HarmonyPostfix))!;
        MethodInfo disharmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.UnpatchHarmony_PreservesDisharmonyPatch_DisharmonyPrefix))!;
        MethodInfo disharmonyPostfix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.UnpatchHarmony_PreservesDisharmonyPatch_DisharmonyPostfix))!;
        Patcher.Patch(
            Patch.Prefix.With(disharmonyPrefix).Of(target),
            Patch.Postfix.With(disharmonyPostfix).Of(target));
        harmony.Patch(target, prefix: new HarmonyMethod(harmonyPrefix), postfix: new HarmonyMethod(harmonyPostfix));
        HarmonyCoexistenceTargets.OrderedTarget();
        HarmonyCoexistenceTargets.Events.Clear();

        harmony.Unpatch(target, HarmonyPatchType.All, HarmonyId);
        int result = HarmonyCoexistenceTargets.OrderedTarget();

        Assert.That(result, Is.EqualTo(10));
        Assert.That(HarmonyCoexistenceTargets.Events, Is.EqualTo(new[]
        {
            "disharmony-prefix",
            "target",
            "disharmony-postfix",
        }));
    }

    [Test]
    public void ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch()
    {
        MethodInfo target = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.OrderedTarget))!;
        MethodInfo harmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch_HarmonyPrefix))!;
        MethodInfo harmonyPostfix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch_HarmonyPostfix))!;
        MethodInfo firstDisharmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch_FirstDisharmonyPrefix))!;
        MethodInfo secondDisharmonyPrefix = typeof(HarmonyCoexistencePatches)
            .GetMethod(nameof(HarmonyCoexistencePatches.ReapplyDisharmony_WithHarmonyPatchInstalled_UsesNewPatch_SecondDisharmonyPrefix))!;
        harmony.Patch(target, prefix: new HarmonyMethod(harmonyPrefix), postfix: new HarmonyMethod(harmonyPostfix));
        PatchHandle firstHandle = Patcher.Patch(Patch.Prefix.With(firstDisharmonyPrefix).Of(target));
        HarmonyCoexistenceTargets.OrderedTarget();
        Patcher.Unpatch(firstHandle);
        Patcher.Patch(Patch.Prefix.With(secondDisharmonyPrefix).Of(target));
        HarmonyCoexistenceTargets.Events.Clear();

        int result = HarmonyCoexistenceTargets.OrderedTarget();

        Assert.That(result, Is.EqualTo(10));
        Assert.That(HarmonyCoexistenceTargets.Events, Is.EqualTo(new[]
        {
            "harmony-prefix",
            "second-disharmony-prefix",
            "target",
            "harmony-postfix",
        }));
    }

    [Test]
    public void HarmonyTranspiler_DisharmonyOuterPostfix_ObservesTranspiledResult()
    {
        HarmonyCoexistencePatches.ObservedResult = 0;
        MethodInfo target = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.TranspilerOuterTarget))!;
        MethodInfo harmonyTranspiler = typeof(HarmonyCoexistencePatches).GetMethod(nameof(HarmonyCoexistencePatches
            .HarmonyTranspiler_DisharmonyOuterPostfix_ObservesTranspiledResult_HarmonyTranspiler))!;
        MethodInfo disharmonyPostfix = typeof(HarmonyCoexistencePatches).GetMethod(nameof(HarmonyCoexistencePatches
            .HarmonyTranspiler_DisharmonyOuterPostfix_ObservesTranspiledResult_DisharmonyPostfix))!;
        harmony.Patch(target, transpiler: new HarmonyMethod(harmonyTranspiler));
        Patcher.Patch(Patch.Postfix.With(disharmonyPostfix).Of(target));

        int result = HarmonyCoexistenceTargets.TranspilerOuterTarget();

        Assert.That(result, Is.EqualTo(2));
        Assert.That(HarmonyCoexistencePatches.ObservedResult, Is.EqualTo(2));
    }

    [Test]
    public void HarmonyTranspiler_DisharmonyInnerPrefix_ObservesTranspiledArgument()
    {
        HarmonyCoexistencePatches.ObservedArgument = 0;
        MethodInfo outer = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.TranspilerInnerTarget))!;
        MethodInfo inner = typeof(HarmonyCoexistenceTargets)
            .GetMethod(nameof(HarmonyCoexistenceTargets.Inner))!;
        MethodInfo harmonyTranspiler = typeof(HarmonyCoexistencePatches).GetMethod(nameof(HarmonyCoexistencePatches
            .HarmonyTranspiler_DisharmonyInnerPrefix_ObservesTranspiledArgument_HarmonyTranspiler))!;
        MethodInfo disharmonyPrefix = typeof(HarmonyCoexistencePatches).GetMethod(nameof(HarmonyCoexistencePatches
            .HarmonyTranspiler_DisharmonyInnerPrefix_ObservesTranspiledArgument_DisharmonyPrefix))!;
        harmony.Patch(outer, transpiler: new HarmonyMethod(harmonyTranspiler));
        Patcher.Patch(Patch.Prefix.Inner(inner).With(disharmonyPrefix).Of(outer));

        int result = HarmonyCoexistenceTargets.TranspilerInnerTarget();

        Assert.That(result, Is.EqualTo(2));
        Assert.That(HarmonyCoexistencePatches.ObservedArgument, Is.EqualTo(2));
    }
}
