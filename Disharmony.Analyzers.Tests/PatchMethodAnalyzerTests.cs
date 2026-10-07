using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Disharmony.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

using static Disharmony.Analyzers.Tests.AnalyzerTestHelper;

namespace Disharmony.Analyzers.Tests;

public class PatchMethodAnalyzerTests
{
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M<T>() {} }", "DISHARMONY0001")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C<T> { [Prefix] static void M() {} }", "DISHARMONY0001")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C<T> { [Patch, Target(typeof(object), \"M\")] class Nested { [Postfix] static void M() {} } }", "DISHARMONY0001")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] void M() {} }", "DISHARMONY0002")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] void M() {} }", "DISHARMONY0002")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static int M() => 0; }", "DISHARMONY0003")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool? M() => null; }", "DISHARMONY0003")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { static bool b; [Prefix] static ref bool M() => ref b; }", "DISHARMONY0003")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { static bool b; [Prefix] static ref readonly bool M() => ref b; }", "DISHARMONY0003")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static bool M() => true; }", "DISHARMONY0004")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static bool M() => true; }", "DISHARMONY0005")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static int M() => 0; }", "DISHARMONY0005")]
    [TestCase("[PatchOptions(PatchOptions.AlwaysRun)] [Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M() => true; }", "DISHARMONY0005")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.Inline | PatchOptions.AlwaysRun)] static bool M() => true; }", "DISHARMONY0005")]
    [TestCase("[PatchOptions(PatchOptions.Default)] [Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static bool M() => true; }", "DISHARMONY0005")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] partial class C { [Prefix] static bool M() => true; } [PatchOptions(PatchOptions.AlwaysRun)] partial class C {}", "DISHARMONY0005")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [global::Disharmony.PrefixAttribute] static int M() => 0; }", "DISHARMONY0003")]
    [TestCase("using P = Disharmony.PrefixAttribute; [Patch, Target(typeof(object), \"M\")] class C { [P] static int M() => 0; }", "DISHARMONY0003")]
    [TestCase("[PatchOptions(PatchOptions.AlwaysRun)] [Patch, Target(typeof(object), \"M\")] class B {} [Patch, Target(typeof(object), \"M\")] class C : B { [Prefix] static bool M() => true; }", "DISHARMONY0005")]
    public async Task InvalidPatchReportsWarningAtMethodName(string source, string expectedId)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].Id, Is.EqualTo(expectedId));
        var text = await diagnostics[0].Location.SourceTree!.GetTextAsync();
        Assert.That(text.ToString(diagnostics[0].Location.SourceSpan), Is.EqualTo("M"));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M() => true; }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M() {} }")]
    [TestCase("[PatchOptions(PatchOptions.AlwaysRun)] [Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.Default)] static bool M() => true; }")]
    [TestCase("[PatchOptions(PatchOptions.AlwaysRun)] [Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.Inline)] static bool M() => true; }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.Inline)] static bool M() => true; }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C<T> { int M<U>() => 0; }")]
    [TestCase("namespace Other { class PrefixAttribute : System.Attribute {} [Patch, Target(typeof(object), \"M\")] class C { [Prefix] int M() => 0; } }")]
    [TestCase("[PatchOptions(PatchOptions.AlwaysRun)] [Patch, Target(typeof(object), \"M\")] class C { [Patch, Target(typeof(object), \"M\")] class Nested { [Prefix] static bool M() => true; } }")]

    [TestCase("[PatchOptions(PatchOptions.AlwaysRun)] [Patch, Target(typeof(object), \"M\")] class B {} [PatchOptions(PatchOptions.Default)] [Patch, Target(typeof(object), \"M\")] class C : B { [Prefix] static bool M() => true; }")]
    [TestCase("namespace HarmonyLib { class HarmonyPostfix : System.Attribute {} [Patch, Target(typeof(object), \"M\")] class C { [HarmonyPostfix] static int M() => 0; } }")]
    public async Task ValidOrUnrelatedCodeDoesNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }

    [Test]
    public async Task IndependentViolationsAreAllReported()
    {
        var diagnostics = await Analyze("[Patch, Target(typeof(object), \"M\")] class C<T> { [Postfix] int M() => 0; }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0001", "DISHARMONY0002", "DISHARMONY0004"]));
    }

    [Test]
    public async Task MissingDisharmonyReferenceDoesNotWarn()
    {
        var compilation = CSharpCompilation.Create("Test",
            [CSharpSyntaxTree.ParseText("class C { int M<T>() => 0; }")],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = await compilation.WithAnalyzers(
            [new PatchAnalyzer()]).GetAnalyzerDiagnosticsAsync();
        Assert.That(diagnostics, Is.Empty);
    }

    [Test]
    public async Task OverriddenMethodInheritsPatchAttribute()
    {
        var diagnostics = await Analyze("[Patch, Target(typeof(object), \"M\")] class B { [Prefix] public virtual void M() {} } [Patch, Target(typeof(object), \"M\")] class C : B { public override void M() {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0002", "DISHARMONY0002"]));
    }

    [Test]
    public async Task CustomPatchAttributeIsIgnored()
    {
        var diagnostics = await Analyze("[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false)] class CustomAttribute : PrefixAttribute {} [Patch, Target(typeof(object), \"M\")] class B { [Custom] public virtual void M() {} } [Patch, Target(typeof(object), \"M\")] class C : B { public override void M() {} }");
        Assert.That(diagnostics, Is.Empty);
    }

    [TestCase("class C { [Prefix, Target(typeof(object), \"M\")] static void M() {} }", "DISHARMONY0006")]
    [TestCase("class C { [Postfix, Targets(typeof(object), \"M\")] static void M() {} }", "DISHARMONY0006")]
    [TestCase("[Patch] class Outer { class C { [Prefix, Target(typeof(object), \"M\")] static void M() {} } }", "DISHARMONY0006")]
    [TestCase("[HarmonyLib.HarmonyPatch] class Outer { class C { [Prefix, Target(typeof(object), \"M\")] static void M() {} } }", "DISHARMONY0006")]
    [TestCase("class C { [HarmonyLib.HarmonyPatch, Prefix, Target(typeof(object), \"M\")] static void M() {} }", "DISHARMONY0006")]
    [TestCase("[Patch] class C { [Prefix] static void M() {} }", "DISHARMONY0007")]
    [TestCase("[HarmonyLib.HarmonyPatch] class C { [Postfix] static void M() {} }", "DISHARMONY0007")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class Outer { [Patch] class C { [Prefix] static void M() {} } }", "DISHARMONY0007")]
    [TestCase("[Patch] class C { [Prefix, Inner(typeof(object), \"M\")] static void M() {} }", "DISHARMONY0007")]
    [TestCase("class C { [Target(typeof(object), \"M\")] static void M() {} }", "DISHARMONY0008")]
    [TestCase("[Patch] class C { [Targets(typeof(object), \"M\")] static void M() {} }", "DISHARMONY0008")]
    [TestCase("class C { [Inner(typeof(object), \"M\")] static void M() {} }", "DISHARMONY0008")]
    [TestCase("class C { [InnerConstant(1)] static void M() {} }", "DISHARMONY0008")]
    [TestCase("class C { [Priority] static void M() {} }", "DISHARMONY0008")]
    [TestCase("class C { [PatchOptions(PatchOptions.Default)] static void M() {} }", "DISHARMONY0008")]
    [TestCase("class C { [Target(typeof(object), \"M\"), Inner(typeof(object), \"M\"), Priority] static void M() {} }", "DISHARMONY0008")]
    [TestCase("using T = Disharmony.TargetAttribute; class C { [T] static void M() {} }", "DISHARMONY0008")]
    [TestCase("class C { [global::Disharmony.TargetsAttribute] static void M() {} }", "DISHARMONY0008")]
    public async Task DiscoveryViolationReportsWarningAtMethodName(string source, string expectedId)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].Id, Is.EqualTo(expectedId));
        Assert.That(diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
        var text = await diagnostics[0].Location.SourceTree!.GetTextAsync();
        Assert.That(text.ToString(diagnostics[0].Location.SourceSpan), Is.EqualTo("M"));
    }

    [TestCase("[Patch] class C { [Prefix, Target(typeof(object), \"M\")] static void M() {} }")]
    [TestCase("[HarmonyLib.HarmonyPatch] class C { [Postfix, Targets(typeof(object), \"M\")] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M() {} }")]
    [TestCase("[Patch, Targets(typeof(object), \"M\")] class C { [Postfix] static void M() {} }")]
    [TestCase("[Patch, Targets(typeof(object), \"M\")] class B {} class C : B { [Prefix] static void M() {} }")]
    [TestCase("[HarmonyLib.HarmonyPatch] class B {} class C : B { [Prefix, Target(typeof(object), \"M\")] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] partial class C {} partial class C { [Prefix] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\"), Priority, PatchOptions(PatchOptions.AlwaysRun)] class C { static void M() {} }")]
    [TestCase("class C { static void M([Argument] int value) {} }")]
    [TestCase("class C { [System.Obsolete] static void M() {} }")]
    [TestCase("class C { [HarmonyLib.HarmonyPatch] static void M() {} }")]
    [TestCase("class TargetAttribute : System.Attribute {} class C { [Target] static void M() {} }")]
    [TestCase("class C : B { [Prefix, Target(typeof(object), \"M\")] static void M() {} } [Patch] class B {}")]
    [TestCase("[Patch] class C { [Prefix, Target(typeof(C), \"DoesNotExist\")] static void M() {} }")]
    [TestCase("[Patch] class C { [Prefix, Target(typeof(object), \"M\"), Targets(typeof(object), \"M\")] static void M() {} }")]
    public async Task DiscoverablePatchesAndUnattributedHelpersDoNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }

    [Test]
    public async Task MissingClassAndTargetAreBothReported()
    {
        var diagnostics = await Analyze("class C { [Prefix] static void M() {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0006", "DISHARMONY0007"]));
    }

    [Test]
    public async Task InheritedMethodAttributeDoesNotWarnOnUnattributedOverride()
    {
        var diagnostics = await Analyze("class B { [Target(typeof(object), \"M\")] public virtual void M() {} } class C : B { public override void M() {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0008"]));
        Assert.That(diagnostics[0].Location.SourceSpan.Start, Is.LessThan(
            diagnostics[0].Location.SourceTree!.ToString().IndexOf("class C", StringComparison.Ordinal)));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Postfix] static void M() {} }", "DISHARMONY0009", "M")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"M\"), InnerConstant(1)] static void M() {} }", "DISHARMONY0010", "M")]
    [TestCase("[Patch] class C { [Prefix, Target(\"M\")] static void M() {} }", "DISHARMONY0011", "Target(\"M\")")]
    [TestCase("[Patch] class C { [Prefix, Targets(\"M\")] static void M() {} }", "DISHARMONY0011", "Targets(\"M\")")]
    [TestCase("[HarmonyLib.HarmonyPatch(\"M\")] class C { [Prefix, Target(\"M\")] static void M() {} }", "DISHARMONY0011", "Target(\"M\")")]
    [TestCase("[Patch(typeof(object)), Target(\"M\")] class C { [Prefix, Inner(null, \"M\")] static void M() {} }", "DISHARMONY0011", "Inner(null, \"M\")")]
    [TestCase("[Target(\"M\")] class B {} [Patch] class C : B { [Prefix, Target(typeof(object), \"M\")] static void M() {} }", "DISHARMONY0011", "Target(\"M\")")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(null)] static void M() {} }", "DISHARMONY0012", "InnerConstant(null)")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { const string Value = null; [Prefix, InnerConstant(Value)] static void M() {} }", "DISHARMONY0012", "InnerConstant(Value)")]
    [TestCase("[HarmonyLib.HarmonyPatch(typeof(object)), HarmonyLib.HarmonyPatch(\"M\")] class C {}", "DISHARMONY0014", "C")]
    [TestCase("[Patch(typeof(object))] class C { [Prefix, Target] static void M() {} }", "DISHARMONY0015", "Target")]
    [TestCase("[Patch] class C { [Prefix, Targets(typeof(object))] static void M() {} }", "DISHARMONY0015", "Targets(typeof(object))")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object))] static void M() {} }", "DISHARMONY0015", "Inner(typeof(object))")]
    public async Task RegistryViolationReportsWarningAtDeclaration(string source, string expectedId, string expectedSpan)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].Id, Is.EqualTo(expectedId));
        var text = await diagnostics[0].Location.SourceTree!.GetTextAsync();
        Assert.That(text.ToString(diagnostics[0].Location.SourceSpan), Is.EqualTo(expectedSpan));
    }

    [TestCase("[Patch] class C { [Prefix, Target(\"Namespace.Type:M\")] static void M() {} }")]
    [TestCase("[Patch] class C { [Prefix, Targets(\"Namespace.Type.M\")] static void M() {} }")]
    [TestCase("[Patch] class C { [Prefix, Target(\"Namespace.Type.M\")] static void M() {} }")]
    [TestCase("[Patch] class C { [Prefix, Targets(\"Namespace.Type:M\")] static void M() {} }")]
    [TestCase("[Patch(typeof(object))] class C { [Prefix, Target(\"M\")] static void M() {} }")]
    [TestCase("[HarmonyLib.HarmonyPatch(typeof(object))] class C { [Prefix, Target(\"M\")] static void M() {} }")]
    [TestCase("[HarmonyLib.HarmonyPatch(\"RuntimeType\", \"M\", HarmonyLib.MethodType.Normal)] class C { [Prefix, Target(\"M\")] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(null, \"Namespace.Type:M\")] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(null, \"Namespace.Type.M\")] static void M() {} }")]
    [TestCase("[Patch] class C { [Prefix, Target(typeof(object), memberType: MemberType.Constructor)] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), memberType: MemberType.Constructor)] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1L)] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1F)] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1D)] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(\"\")] static void M() {} }")]
    [TestCase("[Patch(typeof(object))] class B {} [Patch] class C : B { [Prefix, Target(typeof(object), \"M\")] static void M() {} }")]
    [TestCase("[Patch, Target(typeof(object), \"A\"), Targets(typeof(object), \"B\")] class C { [Prefix, Target(typeof(object), \"C\")] static void M() {} }")]
    public async Task SupportedOrRuntimeDependentRegistryMetadataDoesNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }

    [Test]
    public async Task NonMultipleInheritedPatchAttributeIsNotCountedTwice()
    {
        var diagnostics = await Analyze("[Patch, Target(typeof(object), \"M\")] class B { [Prefix] public virtual void M() {} } class C : B { [Prefix] public override void M() {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0002", "DISHARMONY0002"]));
    }

    [Test]
    public async Task MultipleInnerAttributesAreCheckedEvenWithoutPatchType()
    {
        var diagnostics = await Analyze("[Patch] class C { [Inner(typeof(object), \"M\"), InnerConstant(1)] static void M() {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0008", "DISHARMONY0010"]));
    }

    [TestCase("[Patch, HarmonyLib.HarmonyPatch] class C {}", "DISHARMONY0014")]
    [TestCase("[Patch(typeof(object)), HarmonyLib.HarmonyPatch(typeof(object))] class C {}", "DISHARMONY0014")]
    [TestCase("[Patch, HarmonyLib.HarmonyPatch(typeof(object)), HarmonyLib.HarmonyPatch(\"M\")] class C {}", "DISHARMONY0014")]
    [TestCase("[Patch] class B {} [HarmonyLib.HarmonyPatch] class C : B {}", "DISHARMONY0014")]
    [TestCase("[HarmonyLib.HarmonyPatch] class B {} [Patch] class C : B {}", "DISHARMONY0014")]
    [TestCase("[Patch] partial class C {} [HarmonyLib.HarmonyPatch] partial class C {}", "DISHARMONY0014")]
    public async Task MixedDiscoveryMetadataReportsWarningsOnClass(string source, string expectedIds)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(expectedIds.Split(',')));
        foreach (var diagnostic in diagnostics)
        {
            Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Warning));
            var text = await diagnostic.Location.SourceTree!.GetTextAsync();
            Assert.That(text.ToString(diagnostic.Location.SourceSpan), Is.EqualTo("C"));
        }
    }

    [TestCase("[Patch, HarmonyLib.HarmonyPatchCategory(\"test\")] class C {}")]
    [TestCase("[HarmonyLib.HarmonyPatch, Category(\"test\")] class C {}")]
    [TestCase("[Patch] class Outer { [HarmonyLib.HarmonyPatch] class C {} }")]
    [TestCase("[Category(\"test\")] class Outer { [HarmonyLib.HarmonyPatchCategory(\"test\")] class C {} }")]
    [TestCase("class PatchAttribute : System.Attribute {} [Patch, HarmonyLib.HarmonyPatch] class C {}")]
    [TestCase("class CategoryAttribute : System.Attribute {} [Category, HarmonyLib.HarmonyPatchCategory(\"test\")] class C {}")]
    public async Task SeparateDiscoveryMetadataDoesNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }

}
