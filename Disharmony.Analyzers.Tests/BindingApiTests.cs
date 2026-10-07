using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using static Disharmony.Analyzers.Tests.AnalyzerTestHelper;

namespace Disharmony.Analyzers.Tests;

public class BindingApiTests
{
    [TestCase("[Arguments] ref object[] value", "DISHARMONY0036")]
    [TestCase("[Arguments] in object[] value", "DISHARMONY0036")]
    [TestCase("ref object[] __args", "DISHARMONY0036")]
    [TestCase("[MemberInfo] ref System.Reflection.MemberInfo value", "DISHARMONY0036")]
    [TestCase("[MemberInfo] in System.Reflection.MemberInfo value", "DISHARMONY0036")]
    [TestCase("[Arguments] string[] value", "DISHARMONY0021")]
    [TestCase("[Arguments] int value", "DISHARMONY0021")]
    [TestCase("[Arguments(Scope.Inner)] object[] value", "DISHARMONY0017")]
    [TestCase("[MemberInfo(Scope.Inner)] System.Reflection.MemberInfo value", "DISHARMONY0017")]
    [TestCase("[BaseMethod(Scope.Inner)] System.Action value", "DISHARMONY0017")]
    [TestCase("[Argument, Arguments] object[] value", "DISHARMONY0016")]
    [TestCase("[MemberInfo, Arguments] object value", "DISHARMONY0016")]
    public async Task InvalidBindingsReportErrors(string parameters, string id)
    {
        var diagnostics = await Analyze("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(" + parameters + ") {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(new[] { id }));
        Assert.That(diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Arguments] out object[] value) { value = null; } }", "DISHARMONY0036")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([MemberInfo] out object value) { value = null; } }", "DISHARMONY0036")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AllowUnsafe)] static void M([Arguments] string value) {} }", "DISHARMONY0021")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M([MemberInfo(Scope.Any)] object value) {} }", "DISHARMONY0037")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(System.Action __base) {} }", "DISHARMONY0037")]
    public async Task InvalidBindingsWithBodiesOrOptionsReportErrors(string source, string id)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(new[] { id }));
        Assert.That(diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
    }

    [TestCase("object[] __args, [Arguments] object[] other")]
    [TestCase("[MemberInfo] object a, [MemberInfo(Scope.Outer)] object b")]
    [TestCase("System.Action __base, [BaseMethod(Scope.Inner)] System.Action other")]
    [TestCase("[Method(typeof(object), \"M\")] System.Action a, [Method(typeof(object), \"M\", virtualCall: true)] System.Action b")]
    [TestCase("[Field(typeof(object), \"field\")] int a, [Field(typeof(object), \"field\", Scope.Any)] int b")]
    public async Task EquivalentBindingsWarn(string parameters)
    {
        var diagnostics = await Analyze("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M(" + parameters + ") {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(new[] { "DISHARMONY0028", "DISHARMONY0028" }));
    }

    [TestCase("[Arguments] object[] a, [Arguments(Scope.Outer)] object[] b")]
    [TestCase("[MemberInfo] object a, [MemberInfo(Scope.Any)] object b")]
    [TestCase("System.Action __base, [BaseMethod(Scope.Outer)] System.Action other")]
    [TestCase("[Method(\"M\")] System.Action a, [Method(\"M\", virtualCall: false)] System.Action b")]
    [TestCase("[Method(\"M\")] System.Action a, [Method(\"M\")] System.Action<int> b")]
    [TestCase("[Method(typeof(object), \"M\")] System.Action a, [Method(typeof(string), \"M\")] System.Action b")]
    [TestCase("[Field(typeof(object), \"field\")] int a, [Field(typeof(string), \"field\")] int b")]
    public async Task DistinctBindingsDoNotWarn(string parameters)
    {
        Assert.That(await Analyze("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, Inner(typeof(object), \"I\")] static void M(" + parameters + ") {} }"), Is.Empty);
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(object[] __args) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Arguments] object value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Arguments] System.Array value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Arguments] System.Collections.IEnumerable value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([Argument(\"x\")] int __args) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([MemberInfo] System.Reflection.MethodInfo value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M(object[] __args, [MemberInfo] object value, [BaseMethod(Scope.Outer)] System.Action action) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M([Field(typeof(string), \"Empty\", Scope.Inner)] string value) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, InnerConstant(1)] static void M([Field(\"System.String.Empty\", Scope.Inner)] string value) {} }")]
    public async Task SupportedOrUnresolvedBindingsDoNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }
}