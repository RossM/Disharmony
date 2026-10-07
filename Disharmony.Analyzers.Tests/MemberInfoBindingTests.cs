using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using static Disharmony.Analyzers.Tests.AnalyzerTestHelper;

namespace Disharmony.Analyzers.Tests;

public class MemberInfoBindingTests
{
    [TestCase("string", "PatchOptions.Default", "DISHARMONY0038")]
    [TestCase("int", "PatchOptions.Default", "DISHARMONY0038")]
    [TestCase("System.Reflection.PropertyInfo", "PatchOptions.Default", "DISHARMONY0038")]
    [TestCase("System.Reflection.EventInfo", "PatchOptions.Default", "DISHARMONY0038")]
    [TestCase("System.Type", "PatchOptions.Default", "DISHARMONY0038")]
    [TestCase("int", "PatchOptions.AllowUnsafe", "DISHARMONY0038")]
    [TestCase("System.Reflection.FieldInfo", "PatchOptions.Default", "")]
    [TestCase("System.Reflection.MethodInfo", "PatchOptions.Default", "")]
    [TestCase("System.Reflection.ConstructorInfo", "PatchOptions.Default", "")]
    [TestCase("System.Reflection.MethodBase", "PatchOptions.Default", "")]
    [TestCase("System.Reflection.MemberInfo", "PatchOptions.Default", "")]
    [TestCase("System.Reflection.ICustomAttributeProvider", "PatchOptions.Default", "")]
    [TestCase("object", "PatchOptions.Default", "")]
    [TestCase("string", "PatchOptions.AllowUnsafe", "")]
    public async Task MetadataTypeMustAcceptAtLeastOneSupportedMemberType(string type, string options, string expectedId)
    {
        var source = "[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(" + options +
                     ")] static void M([MemberInfo] " + type + " value) {} }";
        var diagnostics = await Analyze(source);
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(expectedId.Length == 0 ? new string[0] : new[] { expectedId }));
        if (expectedId.Length != 0)
        {
            Assert.That(diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
            var text = await diagnostics[0].Location.SourceTree!.GetTextAsync();
            Assert.That(text.ToString(diagnostics[0].Location.SourceSpan), Is.EqualTo("value"));
        }
    }

    [Test]
    public async Task UnsafeDoesNotPermitByReferenceMetadata()
    {
        var diagnostics = await Analyze("[Patch, Target(typeof(object), \"M\")] class C { [Prefix, PatchOptions(PatchOptions.AllowUnsafe)] static void M([MemberInfo] ref string value) {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(new[] { "DISHARMONY0036" }));
    }

    [Test]
    public async Task UserDefinedConversionDoesNotMakeMetadataCompatible()
    {
        var diagnostics = await Analyze("class Wrapper { public static implicit operator Wrapper(System.Reflection.MethodInfo value) => null; } [Patch, Target(typeof(object), \"M\")] class C { [Prefix] static void M([MemberInfo] Wrapper value) {} }");
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(new[] { "DISHARMONY0038" }));
    }
}