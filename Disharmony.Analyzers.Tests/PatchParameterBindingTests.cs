using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

using static Disharmony.Analyzers.Tests.AnalyzerTestHelper;

namespace Disharmony.Analyzers.Tests;

public class PatchParameterBindingTests
{
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(System.Exception __exception, [Exception] System.Exception other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(object __instance, [Instance(Scope.Outer)] object other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value, [Argument(\"value\")] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument(0)] int a, [Argument(0, Scope.Outer)] int b) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int ___field, [Field(\"field\")] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(System.Action __base, [BaseMethod] System.Action other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Method] System.Action action, [Method(\"action\")] System.Action other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(ref int __state, [State(\"__state\")] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M([Argument(1)] int a, [Argument(1, Scope.Inner)] int b) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M(int value, [Argument(\"value\")] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(int __result, [ReturnValue] in int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(object __caller, [Instance(Scope.Outer)] object outer, int value, [Field] int field, [Argument(0, Scope.Outer)] int x) {} }")]
    public async Task DuplicateBindingsWarnOnEachParameter(string source)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0028", "DISHARMONY0028"]));
        Assert.That(diagnostics.All(d => d.Severity == DiagnosticSeverity.Warning), Is.True);
        Assert.That(diagnostics.Select(d => d.Location.SourceSpan).Distinct().Count(), Is.EqualTo(2));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value, [Argument(0)] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M([Argument(\"x\", Scope.Inner)] int a, [Argument(\"x\", Scope.Outer)] int b, [Argument(\"x\")] int c) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M(object __instance, object __caller) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value, [Field(\"value\")] int field) {} }")]
    [TestCase("[Patch] partial class C { [Prefix, Target(typeof(object), \"A\")] static void A([State(\"key\")] out int a) { a = 1; } } partial class C { [Postfix, Target(typeof(object), \"B\")] static void B([State(\"key\")] in int b) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void A(ref int __state) {} [Prefix] static void B(int __state) {} }")]
    public async Task DistinctBindingsAndWrittenStateDoNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(in int __state) {} static void Helper(ref int __state) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class B { [Prefix] static void A(ref int __state) {} } class C : B { [Postfix] static void B(int __state) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A([State(\"written\")] ref int a) {} [Postfix] static void B([State(\"unwritten\")] int b) {} }")]
    public async Task StateWithoutMatchingPatchWriterWarns(string source)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(["DISHARMONY0029"]));
        Assert.That(diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
    }
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(out int __state) { __state = 1; } }", 1)]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A([State(\"key\")] out int a) { a = 1; } [Postfix] static void B([State(\"key\")] out int b) { b = 2; } }", 2)]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(out int __state) { __state = 1; } static void Helper(int __state) {} }", 1)]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A([State(\"unused\")] out int a, [State(\"used\")] ref int b) { a = 1; } }", 1)]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class B { [Prefix] static void A(out int __state) { __state = 1; } } class C : B { [Postfix] static void B(ref int __state) {} }", 1)]
    public async Task StateOnlyBoundThroughOutWarns(string source, int expectedCount)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics, Has.Length.EqualTo(expectedCount));
        Assert.That(diagnostics.All(d => d.Id == "DISHARMONY0030" && d.Severity == DiagnosticSeverity.Warning), Is.True);
        Assert.That(diagnostics.Select(d => d.Location.SourceSpan).Distinct().Count(), Is.EqualTo(expectedCount));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(out int __state) { __state = 1; } [Postfix] static void B(int __state) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(out int __state) { __state = 1; } [Postfix] static void B([State(\"__state\")] ref int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(ref int __state) {} }")]
    public async Task StateWithReaderDoesNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M(int ___field, [Field(\"field\", Scope.Any)] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M([Field] int field, [Field(\"field\")] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M([Field(\"field\", Scope.Inner)] int a, [Field(\"field\", Scope.Inner)] int b) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, Inner(typeof(object), \"I\")] static void M([Field(\"field\", Scope.Outer)] int a, [Field(\"field\", Scope.Outer)] int b) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int ___field, [Field(\"field\", Scope.Outer)] int other) {} }")]
    public async Task EquivalentFieldBindingsWarnOnBothParameters(string source)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0028", "DISHARMONY0028"]));
        Assert.That(diagnostics.All(d => d.Severity == DiagnosticSeverity.Warning), Is.True);
        Assert.That(diagnostics.Select(d => d.Location.SourceSpan).Distinct().Count(), Is.EqualTo(2));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M(int ___field, [Field(\"field\", Scope.Inner)] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M(int ___field, [Field(\"field\", Scope.Outer)] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M([Field(\"field\", Scope.Any)] int a, [Field(\"field\", Scope.Inner)] int b, [Field(\"field\", Scope.Outer)] int c) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, Inner(typeof(object), \"I\")] static void M([Field] int field, [Field(\"field\", Scope.Inner)] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M([Field(\"first\")] int a, [Field(\"second\")] int b) {} }")]
    public async Task DistinctOrRuntimeDependentFieldBindingsDoNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value, [Argument(\"value\", Scope.Outer)] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Argument(\"value\", Scope.Any)] int a, [Argument(\"value\", Scope.Outer)] int b) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument] int value, [Argument(\"value\", Scope.Outer)] int other) {} }")]
    public async Task OrdinaryPatchNamedArgumentAnyAndOuterWarnAsDuplicates(string source)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(["DISHARMONY0028", "DISHARMONY0028"]));
        Assert.That(diagnostics.All(d => d.Severity == DiagnosticSeverity.Warning), Is.True);
        Assert.That(diagnostics.Select(d => d.Location.SourceSpan).Distinct().Count(), Is.EqualTo(2));
    }
}