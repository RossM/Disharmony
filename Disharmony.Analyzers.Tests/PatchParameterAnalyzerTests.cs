using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

using static Disharmony.Analyzers.Tests.AnalyzerTestHelper;

namespace Disharmony.Analyzers.Tests;

public class PatchParameterAnalyzerTests
{
    [TestCase("[Patch, Target(typeof(object), \"M\"), PatchOptions(PatchOptions.AlwaysRun)] class C { [Prefix, PatchOptions(PatchOptions.Default)] static void M(int __result) {} }", "DISHARMONY0025")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([ReturnValue] int result) {} }", "DISHARMONY0025")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(int __result) {} }", "DISHARMONY0025")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(ref int __result) {} }", "DISHARMONY0025,DISHARMONY0034")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(object __result) {} }", "DISHARMONY0025")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(\"text\")] static void M(in object __result) {} }", "DISHARMONY0025")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([ReturnValue] out int value) { value = 1; } }", "DISHARMONY0025")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int __result) {} }", "DISHARMONY0025")]
    public async Task PrefixResultBindingReportsLikelyMistakes(string source, string expectedIds)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(expectedIds.Split(',')));
        Assert.That(diagnostics.All(d => d.Severity == DiagnosticSeverity.Warning), Is.True);
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M(out int __result) { __result = 1; return false; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M([ReturnValue] out int value) { value = 1; return false; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M(int __result) => false; }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M(in int __result) => true; }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M([ReturnValue] int value) => false; }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M([ReturnValue] in int value) => true; }")]
    public async Task PrefixResultDeclarationsDoNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }

    [TestCase("class CustomAttribute : ParameterBindingAttribute { public CustomAttribute() : base(Scope.Inner) {} } [Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Custom] int value) {} }")]
    [TestCase("class CustomAttribute : ArgumentAttribute {} [Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument, Custom] int value) {} }")]
    public async Task CustomParameterAttributesAreIgnored(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument, Instance] int value) {} }", "DISHARMONY0016", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(object __caller) {} }", "DISHARMONY0017", "__caller")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Instance(Scope.Inner)] object value) {} }", "DISHARMONY0017", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument(0, Scope.Inner)] int value) {} }", "DISHARMONY0017", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument(\"x\", Scope.Inner)] int value) {} }", "DISHARMONY0017", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Field(Scope.Inner)] int value) {} }", "DISHARMONY0017", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Method(Scope.Inner)] System.Action value) {} }", "DISHARMONY0017", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static void M(int __result) {} }", "DISHARMONY0018", "__result")]
    [TestCase("[Patch, Target(typeof(object), \"M\"), PatchOptions(PatchOptions.AlwaysRun)] class C { [Prefix] static void M([ReturnValue] int value) {} }", "DISHARMONY0018", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun | PatchOptions.AllowUnsafe)] static void M(int __result) {} }", "DISHARMONY0018", "__result")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1), PatchOptions(PatchOptions.AlwaysRun)] static void M(int __result) {} }", "DISHARMONY0018", "__result")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(System.Exception __exception) {} }", "DISHARMONY0019", "__exception")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static void M([Exception] System.Exception value) {} }", "DISHARMONY0019", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\"), PatchOptions(PatchOptions.AlwaysRun)] class C { [Postfix, PatchOptions(PatchOptions.Default)] static void M(System.Exception __exception) {} }", "DISHARMONY0019", "__exception")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(object __base) {} }", "DISHARMONY0020", "__base")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([BaseMethod] System.Delegate value) {} }", "DISHARMONY0020", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Method] System.MulticastDelegate value) {} }", "DISHARMONY0020", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Method] ref System.Action value) {} }", "DISHARMONY0020", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(in System.Action __base) {} }", "DISHARMONY0020", "__base")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Method] out System.Action value) { value = null; } }", "DISHARMONY0020", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AllowUnsafe)] static void M([Method] object value) {} }", "DISHARMONY0020", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(string __exception) {} }", "DISHARMONY0021", "__exception")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(System.InvalidOperationException __exception) {} }", "DISHARMONY0021", "__exception")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(ref object __exception) {} }", "DISHARMONY0021", "__exception")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(out object __exception) { __exception = null; } }", "DISHARMONY0021", "__exception")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun | PatchOptions.AllowUnsafe)] static void M(int __exception) {} }", "DISHARMONY0021", "__exception")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(object __instance) {} }", "DISHARMONY0024", "__instance")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M([Instance] object value) {} }", "DISHARMONY0024", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M([Argument(0)] int value) {} }", "DISHARMONY0024", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M([Argument(\"x\", Scope.Inner)] int value) {} }", "DISHARMONY0024", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M([Field(Scope.Inner)] int value) {} }", "DISHARMONY0024", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(long __result) {} }", "DISHARMONY0021,DISHARMONY0025", "__result")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(ref object __result) {} }", "DISHARMONY0021,DISHARMONY0025,DISHARMONY0034", "__result")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(in object __result) {} }", "DISHARMONY0021,DISHARMONY0025", "__result")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, InnerConstant(\"text\")] static void M(ref object __result) {} }", "DISHARMONY0021", "__result")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int __resut) {} }", "DISHARMONY0027", "__resut")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(int __Result) {} }", "DISHARMONY0027", "__Result")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(int __) {} }", "DISHARMONY0027", "__")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int __0) {} }", "DISHARMONY0027", "__0")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M(ref int __result) { __result = 1; return false; } }", "DISHARMONY0034", "__result")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static bool M([ReturnValue] ref int value) { value = 1; return false; } }", "DISHARMONY0034", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(out int value, [Argument(100)] int other) { value = 1; } }", "DISHARMONY0035", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(out int value) { value = 1; } }", "DISHARMONY0035", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Argument(\"argument\")] out int value) { value = 1; } }", "DISHARMONY0035", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Argument(0)] out int value) { value = 1; } }", "DISHARMONY0035", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, Inner(typeof(object), \"I\")] static void M([Argument(Scope.Inner)] out int value) { value = 1; } }", "DISHARMONY0035", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun | PatchOptions.AllowUnsafe)] static void M(out int value) { value = 1; } }", "DISHARMONY0035", "value")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Argument] out int __result) { __result = 1; } }", "DISHARMONY0035", "__result")]
    public async Task InvalidParameterBindingReportsWarningAtParameter(string source, string expectedId, string expectedName)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(expectedId.Split(',')));
        var text = await diagnostics[0].Location.SourceTree!.GetTextAsync();
        Assert.That(text.ToString(diagnostics[0].Location.SourceSpan), Is.EqualTo(expectedName));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument] object __caller) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static void M([Argument(\"result\")] int __result) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Field] int __exception) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Instance] object __base) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"Inner\")] static void M(object __caller, [Argument(Scope.Inner)] int x) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Instance(Scope.Outer)] object value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M([Exception] System.Exception value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(object __exception) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(in object __exception) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(ref System.Exception __exception) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M(out System.InvalidOperationException __exception) { __exception = null; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun | PatchOptions.AllowUnsafe)] static void M(string __exception) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun | PatchOptions.AllowUnsafe)] static void M(ref object __exception) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(System.Action __base, [Method] System.Func<int, string> method) {} }")]
    [TestCase("delegate void CustomDelegate(); [Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Method] CustomDelegate value) {} }")]
    [TestCase("class C { static void M(object __caller, System.Exception __exception, [Instance(Scope.Inner)] object value) {} }")]
    [TestCase("class ArgumentAttribute : System.Attribute {} [Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument, Instance] object value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int ___field, int ____field, int _value, int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument] int __custom, [Field(\"field\")] int __other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([ReturnValue] int __resut) {} }")]
    [TestCase("class C { static void M(int __resut) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(in int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Argument(0)] int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument] out int value) { value = 1; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([ReturnValue] ref int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Field] out int value) { value = 1; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(ref int value, [Argument(100)] int other) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Argument(\"argument\")] ref int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, Inner(typeof(object), \"I\")] static void M([Argument(Scope.Inner)] ref int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun | PatchOptions.AllowUnsafe)] static void M(ref int value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Argument] ref int __result) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([Argument(0)] ref int value) {} }")]
    public async Task ValidOrTargetDependentParameterBindingDoesNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(int __state) {} [Postfix] static void B(string __state) {} }", "DISHARMONY0022,DISHARMONY0022,DISHARMONY0029,DISHARMONY0029")]
    [TestCase("[Patch] class C { [Prefix, Target(typeof(object), \"A\")] static void A([State(\"shared\")] int a) {} [Postfix, Target(typeof(object), \"B\")] static void B([State(\"shared\")] object b) {} }", "DISHARMONY0022,DISHARMONY0022,DISHARMONY0029,DISHARMONY0029")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A([State(\"shared\")] int a, [State(\"shared\")] string b) {} }", "DISHARMONY0022,DISHARMONY0022,DISHARMONY0029,DISHARMONY0029,DISHARMONY0028,DISHARMONY0028")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] partial class C { [Prefix] static void A(int __state) {} } partial class C { [Postfix] static void B(string __state) {} }", "DISHARMONY0022,DISHARMONY0022,DISHARMONY0029,DISHARMONY0029")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A([State(\"__state\")] int a) {} [Postfix] static void B(string __state) {} }", "DISHARMONY0022,DISHARMONY0022,DISHARMONY0029,DISHARMONY0029")]
    public async Task ConflictingStateTypesWarnOnBothParameters(string source, string expectedIds)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(expectedIds.Split(',')));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(ref int __state) {} [Postfix] static void B(int __state) {} }", "")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A([State(\"a\")] int a) {} [Postfix] static void B([State(\"b\")] string b) {} }", "DISHARMONY0029,DISHARMONY0029")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A([State(null)] int a) {} [Postfix] static void B([State] int a) {} }", "DISHARMONY0029,DISHARMONY0029")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(int __state) {} }", "DISHARMONY0029")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(int __state) {} [Postfix] static void B([Argument] string __state) {} }", "DISHARMONY0029")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class B { [Prefix] static void A(int __state) {} } class C : B { [Postfix] static void M(string __state) {} }", "DISHARMONY0029,DISHARMONY0029")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A(int __state) {} [Patch, Target(typeof(object), \"M\")] class Nested { [Postfix] static void B(string __state) {} } }", "DISHARMONY0029,DISHARMONY0029")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void A((int a, string b) __state) {} [Postfix] static void B((int x, string y) __state) {} }", "DISHARMONY0029,DISHARMONY0029")]
    public async Task IndependentOrCompatibleStateBindingsCheckForWriters(string source, string expectedIds)
    {
        Assert.That((await Analyze(source)).Select(d => d.Id), Is.EquivalentTo(expectedIds.Length == 0 ? [] : expectedIds.Split(',')));
    }
}
