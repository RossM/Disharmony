using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Disharmony.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Disharmony.Analyzers.Tests;

internal static class AnalyzerTestHelper
{
    // Minimal metadata contract: no game or patch execution is needed to analyze source.
    private const string Attributes = """
        namespace Disharmony
        {
            public enum MemberType { Any, Method, Getter, Setter, Constructor }
            public enum PatchType { Prefix, Postfix }
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class PatchAttribute : System.Attribute
            {
                public PatchAttribute(System.Type type = null) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class CategoryAttribute : System.Attribute
            {
                public CategoryAttribute(string category) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true)]
            public class TargetAttribute : System.Attribute
            {
                public TargetAttribute(System.Type type, string methodName = null, MemberType memberType = MemberType.Any) { }
                public TargetAttribute(string methodName = null, MemberType memberType = MemberType.Any) { }
            }
            public class TargetsAttribute : TargetAttribute
            {
                public TargetsAttribute(System.Type type, string methodName = null, MemberType memberType = MemberType.Any) { }
                public TargetsAttribute(string methodName = null, MemberType memberType = MemberType.Any) { }
            }
            public abstract class InnerAttributeBase : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class InnerAttribute : InnerAttributeBase
            {
                public InnerAttribute(System.Type type, string memberName = null, MemberType memberType = MemberType.Any) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class InnerConstantAttribute : InnerAttributeBase
            {
                public InnerConstantAttribute(int value) { }
                public InnerConstantAttribute(long value) { }
                public InnerConstantAttribute(float value) { }
                public InnerConstantAttribute(double value) { }
                public InnerConstantAttribute(string value) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method)]
            public class PriorityAttribute : System.Attribute { }
            public enum Scope { Any, Inner, Outer }
            public abstract class ParameterBindingAttribute : System.Attribute
            {
                protected ParameterBindingAttribute(Scope scope) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class ArgumentAttribute : ParameterBindingAttribute
            {
                public ArgumentAttribute(Scope scope = Scope.Any) : base(scope) { }
                public ArgumentAttribute(string name, Scope scope = Scope.Any) : base(scope) { }
                public ArgumentAttribute(int index, Scope scope = Scope.Any) : base(scope) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class InstanceAttribute : ParameterBindingAttribute
            {
                public InstanceAttribute(Scope scope = Scope.Any) : base(scope) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class ReturnValueAttribute : ParameterBindingAttribute
            {
                public ReturnValueAttribute() : base(Scope.Any) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class StateAttribute : ParameterBindingAttribute
            {
                public StateAttribute(string key = null) : base(Scope.Outer) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class FieldAttribute : ParameterBindingAttribute
            {
                public FieldAttribute(Scope scope = Scope.Any) : base(scope) { }
                public FieldAttribute(string name, Scope scope = Scope.Any) : base(scope) { }
                public FieldAttribute(System.Type type, string name, Scope scope = Scope.Any) : base(scope) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class BaseMethodAttribute : ParameterBindingAttribute
            {
                public BaseMethodAttribute(Scope scope = Scope.Any) : base(scope) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class MethodAttribute : ParameterBindingAttribute
            {
                public MethodAttribute(Scope scope = Scope.Any, bool virtualCall = true) : base(scope) { }
                public MethodAttribute(string name, Scope scope = Scope.Any, bool virtualCall = true) : base(scope) { }
                public MethodAttribute(System.Type type, string name = null, Scope scope = Scope.Any, bool virtualCall = true) : base(scope) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class ExceptionAttribute : ParameterBindingAttribute
            {
                public ExceptionAttribute() : base(Scope.Any) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class ArgumentsAttribute : ParameterBindingAttribute
            {
                public ArgumentsAttribute(Scope scope = Scope.Any) : base(scope) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public class MemberInfoAttribute : ParameterBindingAttribute
            {
                public MemberInfoAttribute(Scope scope = Scope.Outer) : base(scope) { }
            }
            [System.Flags]
            public enum PatchOptions { Default = 0, Inline = 1, AlwaysRun = 4, AllowUnsafe = 8 }
            public abstract class PatchTypeAttribute : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class PrefixAttribute : PatchTypeAttribute { }
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class PostfixAttribute : PatchTypeAttribute { }
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method)]
            public class PatchOptionsAttribute : System.Attribute
            {
                public PatchOptionsAttribute(PatchOptions options) { }
            }
        }
        namespace HarmonyLib
        {
            public enum MethodType { Normal, Getter, Setter, Constructor }
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true)]
            public class HarmonyPatch : System.Attribute
            {
                public HarmonyPatch() { }
                public HarmonyPatch(System.Type declaringType) { }
                public HarmonyPatch(string methodName) { }
                public HarmonyPatch(string typeName, string methodName, MethodType methodType) { }
            }
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class HarmonyPatchCategory : System.Attribute
            {
                public HarmonyPatchCategory(string category) { }
            }
        }
        """;

    internal static async Task<ImmutableArray<Diagnostic>> Analyze(string source)
    {
        var compilation = CSharpCompilation.Create("Test",
            [CSharpSyntaxTree.ParseText(Attributes), CSharpSyntaxTree.ParseText("using Disharmony;\n" + source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.That(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty,
            "The fixture must compile before analyzer diagnostics are checked.");
        return await compilation.WithAnalyzers(
            [new PatchAnalyzer()]).GetAnalyzerDiagnosticsAsync();
    }
}
