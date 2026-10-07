using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using static Disharmony.Analyzers.Tests.AnalyzerTestHelper;

namespace Disharmony.Analyzers.Tests;

public class PatchMethodThrowTests
{
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static void M() { throw new System.Exception(); } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M() => throw new System.Exception(); }")]
    [TestCase("[Patch, Target(typeof(object), \"M\"), PatchOptions(PatchOptions.AlwaysRun)] class C { [Postfix] static void M() { throw new System.Exception(); } }")]
    [TestCase("[PatchOptions(PatchOptions.AlwaysRun)] class B {} [Patch, Target(typeof(object), \"M\")] class C : B { [Postfix] static void M() { throw new System.Exception(); } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun | PatchOptions.AllowUnsafe)] static void M() { throw new System.Exception(); } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M() { try { object.Equals(null, null); } catch { throw; } } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AlwaysRun)] static void M(object value) { var x = value ?? throw new System.Exception(); } }")]
    public async Task ExplicitThrowInAlwaysRunPatchWarns(string source)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(["DISHARMONY0032"]));
        Assert.That(diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
        var text = await diagnostics[0].Location.SourceTree!.GetTextAsync();
        Assert.That(text.ToString(diagnostics[0].Location.SourceSpan), Does.StartWith("throw"));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M() { throw new System.Exception(); } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\"), PatchOptions(PatchOptions.AlwaysRun)] class C { [Postfix, PatchOptions(PatchOptions.Default)] static void M() { throw new System.Exception(); } }")]
    [TestCase("[PatchOptions(PatchOptions.AlwaysRun)] class C { static void M() { throw new System.Exception(); } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M() { System.Action action = () => throw new System.Exception(); } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix, PatchOptions(PatchOptions.AlwaysRun)] static void M() { void Local() { throw new System.Exception(); } } }")]
    public async Task ThrowsOutsideAlwaysRunPatchBodyDoNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }
}