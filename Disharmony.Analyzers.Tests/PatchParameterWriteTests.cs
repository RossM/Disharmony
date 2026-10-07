using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using static Disharmony.Analyzers.Tests.AnalyzerTestHelper;

namespace Disharmony.Analyzers.Tests;

public class PatchParameterWriteTests
{
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { value = 1; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M(int value) => value += 1; }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { value++; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { --value; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(string value) { value ??= \"x\"; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { int other; (value, other) = (1, 2); } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { Set(out value); } static void Set(out int x) { x = 1; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { Set(ref value); } static void Set(ref int x) { x = 1; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Postfix] static void M([ReturnValue] int value) { value = 1; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { System.Action action = () => value = 1; action(); } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { void Set() { value = 1; } Set(); } }")]
    public async Task WritingValueParameterWarnsAtWrite(string source)
    {
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(["DISHARMONY0031"]));
        Assert.That(diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
        var text = await diagnostics[0].Location.SourceTree!.GetTextAsync();
        Assert.That(text.ToString(diagnostics[0].Location.SourceSpan), Is.EqualTo("value"));
    }

    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(ref int value) { value++; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(out int value) { value = 1; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { int local = value; local++; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int[] value) { value[0] = 1; } }")]
    [TestCase("class Box { public int Field; public int Property { get; set; } } [Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(Box value) { value.Field = 1; value.Property++; } }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { Read(in value); } static void Read(in int x) {} }")]
    [TestCase("[Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M(int value) { void Set(int local) { local = 1; } Set(value); } }")]
    [TestCase("class C { static void M(int value) { value = 1; } }")]
    public async Task OtherWritesAndReadsDoNotWarn(string source)
    {
        Assert.That(await Analyze(source), Is.Empty);
    }
}