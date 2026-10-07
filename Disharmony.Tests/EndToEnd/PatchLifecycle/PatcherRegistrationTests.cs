namespace Disharmony.Tests.EndToEnd.PatchLifecycle;

public static class PatcherRegistrationPatches
{
    public static int overloadPatchCalls;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.RegistrationResultA))]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.RegistrationResultB))]
    public static void MultipleTargetAttributesPatchEachTarget(ref int __result) => __result = 42;

    [Postfix]
    [Targets(typeof(StaticMethodTargets), nameof(StaticMethodTargets.OverloadedVoid))]
    public static void TargetsAttributePatchesEveryOverload() => overloadPatchCalls++;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    public static void Patch_TargetsOnly_UsesAttributesForInnerPatch(ref int __result) => __result = 42;

    public static void Patch_PatchConfig_UsesExplicitPostfixForEveryTarget(ref int __result) => __result = 42;

    public static bool Patch_PatchConfig_UsesExplicitInnerPrefix() => false;

    public static void Patch_PatchConfig_UsesExplicitInnerPostfix(ref int __result) => __result = 42;

    public static void Patch_PatchConfig_UsesExplicitFieldSetter(ref int value) => value = 42;
}

public static class PatchTypeProcessesEveryPatchMethodOnTypePatches
{
    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.RegistrationResultA))]
    public static void PatchTypeProcessesEveryPatchMethodOnType_First(ref int __result) => __result = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.RegistrationResultB))]
    public static void PatchTypeProcessesEveryPatchMethodOnType_Second(ref int __result) => __result = 42;
}

[Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.RegistrationResultA))]
public static class ClassTargetAttributePatches
{
    public static int firstPatchCalls;
    public static int secondPatchCalls;

    [Postfix]
    public static void ClassTargetAttributeAppliesToEveryPatchMethod_First() => firstPatchCalls++;

    [Postfix]
    public static void ClassTargetAttributeAppliesToEveryPatchMethod_Second() => secondPatchCalls++;
}

[Targets(typeof(StaticMethodTargets), nameof(StaticMethodTargets.OverloadedVoid))]
public static class ClassTargetsAttributePatches
{
    public static int firstPatchCalls;
    public static int secondPatchCalls;

    [Postfix]
    public static void ClassTargetsAttributeAppliesToEveryPatchMethod_First() => firstPatchCalls++;

    [Postfix]
    public static void ClassTargetsAttributeAppliesToEveryPatchMethod_Second() => secondPatchCalls++;
}

[TestFixture]
public sealed class PatcherRegistrationTests : PatchTestBase
{
    private static readonly Assembly DiscoveryAssembly = CreateDiscoveryAssembly();

    private static Assembly CreateDiscoveryAssembly()
    {
        // Isolate assembly discovery, particularly null-category selection, from unrelated test patches.
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("RegistrationPatches"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("RegistrationPatches");
        (string Target, string[] Categories, string[] HarmonyCategories)[] containers =
        [
            (nameof(CategoryTargets.Multiple), ["first", "second"], []),
            (nameof(CategoryTargets.Uncategorized), [], []),
            (nameof(CategoryTargets.Other), ["other"], []),
            (nameof(CategoryTargets.Empty), [""], []),
            (nameof(CategoryTargets.Duplicate), ["duplicate", "duplicate"], []),
            (nameof(CategoryTargets.Mixed), ["disharmony"], ["harmony"]),
        ];

        foreach (var container in containers)
        {
            var type = module.DefineType(container.Target + "Patches",
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
            type.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(PatchAttribute).GetConstructor([typeof(Type)])!, new object[] { typeof(CategoryTargets) }));
            foreach (string category in container.Categories)
                type.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(CategoryAttribute).GetConstructor([typeof(string)])!, new object[] { category }));
            foreach (string category in container.HarmonyCategories)
                type.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(HarmonyPatchCategory).GetConstructor([typeof(string)])!, new object[] { category }));

            var postfix = type.DefineMethod("Postfix", MethodAttributes.Public | MethodAttributes.Static,
                typeof(void), [typeof(string).MakeByRefType()]);
            postfix.DefineParameter(1, ParameterAttributes.None, "__result");
            postfix.SetCustomAttribute(new CustomAttributeBuilder(typeof(PostfixAttribute).GetConstructor(Type.EmptyTypes)!, []));
            postfix.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(TargetAttribute).GetConstructor([typeof(Type), typeof(string)])!,
                new object[] { typeof(CategoryTargets), container.Target }));

            // __result = "patched:" + __result; repeated application is observable in the result.
            ILGenerator il = postfix.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, "patched:");
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldind_Ref);
            il.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)])!);
            il.Emit(OpCodes.Stind_Ref);
            il.Emit(OpCodes.Ret);
            type.CreateType();
        }


        // These containers exercise class-level default target types with both Disharmony and Harmony markers.
        (string Name, Type Marker, Type Category, string CategoryName, string[] Targets)[] registrationContainers =
        [
            ("Preferred", typeof(PatchAttribute), typeof(CategoryAttribute), "preferred-attributes",
                [nameof(StaticMethodTargets.IntIdentity), nameof(StaticMethodTargets.StringIdentity)]),
            ("Included", typeof(HarmonyPatch), typeof(HarmonyPatchCategory), "included",
                [nameof(StaticMethodTargets.RegistrationResultA)]),
            ("Excluded", typeof(HarmonyPatch), typeof(HarmonyPatchCategory), "excluded",
                [nameof(StaticMethodTargets.RegistrationResultB)]),
        ];
        foreach (var container in registrationContainers)
        {
            var type = module.DefineType(container.Name + "Patches",
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
            type.SetCustomAttribute(new CustomAttributeBuilder(
                container.Marker.GetConstructor([typeof(Type)])!, new object[] { typeof(StaticMethodTargets) }));
            type.SetCustomAttribute(new CustomAttributeBuilder(
                container.Category.GetConstructor([typeof(string)])!, new object[] { container.CategoryName }));

            foreach (string targetName in container.Targets)
            {
                Type resultType = typeof(StaticMethodTargets).GetMethod(targetName)!.ReturnType;
                var postfix = type.DefineMethod(targetName + "_Postfix", MethodAttributes.Public | MethodAttributes.Static,
                    typeof(void), [resultType.MakeByRefType()]);
                postfix.DefineParameter(1, ParameterAttributes.None, "__result");
                postfix.SetCustomAttribute(new CustomAttributeBuilder(typeof(PostfixAttribute).GetConstructor(Type.EmptyTypes)!, []));
                // No type on Target: resolution must use the class's Patch/HarmonyPatch attribute.
                postfix.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(TargetAttribute).GetConstructor([typeof(string)])!, new object[] { targetName }));

                ILGenerator il = postfix.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                if (resultType == typeof(string))
                {
                    il.Emit(OpCodes.Ldstr, "patched");
                    il.Emit(OpCodes.Stind_Ref);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I4, 42);
                    il.Emit(OpCodes.Stind_I4);
                }
                il.Emit(OpCodes.Ret);
            }
            type.CreateType();
        }

        return assembly;
    }

    [Test]
    public void PatchCategory_CategoryNames_AreCaseSensitive()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "First");

        Assert.That(CategoryTargets.Multiple(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("original"));
    }

    [Test]
    public void PatchCategory_MultipleCategories_FirstCategoryMatches()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "first");

        Assert.That(CategoryTargets.Multiple(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Other(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("original"));
    }

    [Test]
    public void PatchCategory_MultipleCategories_SecondCategoryMatches()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "second");

        Assert.That(CategoryTargets.Multiple(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Other(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("original"));
    }

    [Test]
    public void PatchCategory_UnmatchedCategory_AppliesNothing()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "missing");

        Assert.That(CategoryTargets.Multiple(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Other(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Empty(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Duplicate(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Mixed(), Is.EqualTo("original"));
    }

    [Test]
    public void PatchCategory_NullCategory_AppliesOnlyUncategorizedClasses()
    {
        Patcher.PatchCategory(DiscoveryAssembly, null);

        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Multiple(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Other(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Empty(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Duplicate(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Mixed(), Is.EqualTo("original"));
    }

    [Test]
    public void PatchCategory_EmptyCategory_DoesNotMeanUncategorized()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "");

        Assert.That(CategoryTargets.Empty(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("original"));
        Assert.That(CategoryTargets.Multiple(), Is.EqualTo("original"));
    }

    [Test]
    public void PatchCategory_DuplicateCategories_AppliesPatchOnlyOnce()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "duplicate");

        Assert.That(CategoryTargets.Duplicate(), Is.EqualTo("patched:original"));
    }

    [Test]
    public void PatchCategory_MixedAttributes_DisharmonyCategoryMatches()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "disharmony");

        Assert.That(CategoryTargets.Mixed(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("original"));
    }

    [Test]
    public void PatchCategory_MixedAttributes_HarmonyCategoryMatches()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "harmony");

        Assert.That(CategoryTargets.Mixed(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("original"));
    }

    [Test]
    public void PatchAll_MultipleCategories_AppliesEveryClassOnlyOnce()
    {
        Patcher.PatchAll(DiscoveryAssembly);

        Assert.That(CategoryTargets.Multiple(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Uncategorized(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Other(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Empty(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Duplicate(), Is.EqualTo("patched:original"));
        Assert.That(CategoryTargets.Mixed(), Is.EqualTo("patched:original"));
    }

    [Test]
    public void PatchTypeProcessesEveryPatchMethodOnType()
    {
        Patcher.PatchAll(typeof(PatchTypeProcessesEveryPatchMethodOnTypePatches));

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(42));
        Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(42));
    }

    [Test]
    public void PatchAttributeMarksClassForAssemblyProcessing()
    {
        Patcher.PatchAll(DiscoveryAssembly);

        Assert.That(StaticMethodTargets.IntIdentity(1), Is.EqualTo(42));
    }

    [Test]
    public void CategoryAttributeMarksClassForCategoryProcessing()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "preferred-attributes");

        Assert.That(StaticMethodTargets.StringIdentity("original"), Is.EqualTo("patched"));
    }

    [Test]
    public void ClassTargetAttributeAppliesToEveryPatchMethod()
    {
        ClassTargetAttributePatches.firstPatchCalls = 0;
        ClassTargetAttributePatches.secondPatchCalls = 0;
        Patcher.PatchAll(typeof(ClassTargetAttributePatches));

        StaticMethodTargets.RegistrationResultA();

        Assert.That(ClassTargetAttributePatches.firstPatchCalls, Is.EqualTo(1));
        Assert.That(ClassTargetAttributePatches.secondPatchCalls, Is.EqualTo(1));
    }

    [Test]
    public void ClassTargetsAttributeAppliesToEveryPatchMethod()
    {
        ClassTargetsAttributePatches.firstPatchCalls = 0;
        ClassTargetsAttributePatches.secondPatchCalls = 0;
        Patcher.PatchAll(typeof(ClassTargetsAttributePatches));

        StaticMethodTargets.OverloadedVoid(1);
        StaticMethodTargets.OverloadedVoid("value");

        Assert.That(ClassTargetsAttributePatches.firstPatchCalls, Is.EqualTo(2));
        Assert.That(ClassTargetsAttributePatches.secondPatchCalls, Is.EqualTo(2));
    }

    [Test]
    public void MultipleTargetAttributesPatchEachTarget()
    {
        MethodInfo patch = typeof(PatcherRegistrationPatches)
            .GetMethod(nameof(PatcherRegistrationPatches.MultipleTargetAttributesPatchEachTarget))!;

        Patcher.Patch(patch);

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(42));
        Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(42));
    }

    [Test]
    public void TargetsAttributePatchesEveryOverload()
    {
        PatcherRegistrationPatches.overloadPatchCalls = 0;
        MethodInfo patch = typeof(PatcherRegistrationPatches)
            .GetMethod(nameof(PatcherRegistrationPatches.TargetsAttributePatchesEveryOverload))!;
        Patcher.Patch(patch);

        StaticMethodTargets.OverloadedVoid(1);
        StaticMethodTargets.OverloadedVoid("value");

        Assert.That(PatcherRegistrationPatches.overloadPatchCalls, Is.EqualTo(2));
    }

    [Test]
    public void Patch_PatchConfig_UsesExplicitPostfixForEveryTarget()
    {
        MethodInfo patch = typeof(PatcherRegistrationPatches)
            .GetMethod(nameof(PatcherRegistrationPatches.Patch_PatchConfig_UsesExplicitPostfixForEveryTarget))!;
        MethodInfo firstTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        MethodInfo secondTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultB))!;

        Patcher.Patch(Patch.Postfix.With(patch), firstTarget, secondTarget);

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(42));
        Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(42));
    }

    [Test]
    public void Patch_PatchConfig_UsesExplicitInnerPrefix()
    {
        MethodInfo patch = typeof(PatcherRegistrationPatches)
            .GetMethod(nameof(PatcherRegistrationPatches.Patch_PatchConfig_UsesExplicitInnerPrefix))!;
        MethodInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets)
            .GetMethod(nameof(OuterStaticMethodTargets.IntResult))!;

        Patcher.Patch(Patch.Prefix.Inner(innerTarget).With(patch).Of(outerTarget));

        Assert.That(OuterStaticMethodTargets.IntResult(), Is.Zero);
    }

    [Test]
    public void Patch_PatchConfig_UsesExplicitInnerPostfix()
    {
        MethodInfo patch = typeof(PatcherRegistrationPatches)
            .GetMethod(nameof(PatcherRegistrationPatches.Patch_PatchConfig_UsesExplicitInnerPostfix))!;
        MethodInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets)
            .GetMethod(nameof(OuterStaticMethodTargets.IntResult))!;

        Patcher.Patch(Patch.Postfix.Inner(innerTarget).With(patch).Of(outerTarget));

        Assert.That(OuterStaticMethodTargets.IntResult(), Is.EqualTo(42));
    }

    [Test]
    public void Patch_PatchConfig_UsesExplicitFieldSetter()
    {
        MethodInfo patch = typeof(PatcherRegistrationPatches)
            .GetMethod(nameof(PatcherRegistrationPatches.Patch_PatchConfig_UsesExplicitFieldSetter))!;
        FieldInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetField(nameof(InnerStaticMethodTargets.Field))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets)
            .GetMethod(nameof(OuterStaticMethodTargets.SetStaticField))!;
        InnerStaticMethodTargets.Field = 0;

        Patcher.Patch(Patch.Prefix.InnerSet(innerTarget).With(patch).Of(outerTarget));

        OuterStaticMethodTargets.SetStaticField(1);

        Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(42));
    }

    [Test]
    public void Patch_PatchConfig_UsesInlineOption()
    {
        MethodInfo patch = typeof(PatcherRegistrationInlinePatches)
            .GetMethod(nameof(PatcherRegistrationInlinePatches.Patch_PatchConfig_UsesInlineOption))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        PatcherRegistrationInlinePatches.ObservedMethod = null;

        Patcher.Patch(Patch.Prefix.With(patch).Options(PatchOptions.Inline).Of(target));

        StaticMethodTargets.RegistrationResultA();

        Assert.That(PatcherRegistrationInlinePatches.ObservedMethod, Is.Not.Null);
        Assert.That(PatcherRegistrationInlinePatches.ObservedMethod, Is.Not.EqualTo(patch));
    }

    [Test]
    public void PatchCategoryProcessesOnlyMatchingCategory()
    {
        Patcher.PatchCategory(DiscoveryAssembly, "included");

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(42));
        Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(2));
    }

    [Test]
    public void PatchAllProcessesAllAssemblyPatchCategories()
    {
        Patcher.PatchAll(DiscoveryAssembly);

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(42));
        Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(42));
    }
}
