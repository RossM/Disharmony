using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Disharmony.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PatchAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        GenericMethod, StaticMethod, PrefixReturn, PostfixReturn, AlwaysRunReturn, MissingPatchClass, MissingTarget, MissingPatchType,
        MultiplePatchTypes, MultipleInnerTargets, MissingTargetType, NullInnerConstant,
        DuplicateDiscoveryAttributes, MissingMemberName, AlwaysRunThrow,
        MultipleParameterBindings, InnerBindingWithoutInnerPatch, AlwaysRunResultBinding, InvalidExceptionBinding,
        InvalidDelegateBinding, IncompatibleBindingType, IncompatibleStateTypes, ConstantBindingUnavailable,
        VoidPrefixResultBinding, UnknownSpecialParameter, DuplicateBinding, StateWithoutWriter,
        StateWithoutReader, WrittenValueParameter, BindingRequiresValue, ConstantMemberUnavailable, IncompatibleMemberInfoType, PrefixResultIsRef, PostfixArgumentIsOut,
    ];

    public static readonly DiagnosticDescriptor GenericMethod = new(
        "DISHARMONY0001", "Patch method must not contain generic parameters",
        "Patch method '{0}' has generic parameters on the method or a containing type; use a non-generic method in a non-generic type",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StaticMethod = new(
        "DISHARMONY0002", "Patch method must be static", 
        "Patch method '{0}' is not static",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PrefixReturn = new(
        "DISHARMONY0003", "Prefix must return bool or void", 
        "Prefix '{0}' must return bool or void",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PostfixReturn = new(
        "DISHARMONY0004", "Postfix must return void", 
        "Postfix '{0}' must return void",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AlwaysRunReturn = new(
        "DISHARMONY0005", "AlwaysRun prefix must return void", 
        "Prefix '{0}' with AlwaysRun must return void",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingPatchClass = new(
        "DISHARMONY0006", "Patch method requires a discoverable containing class",
        "Patch method '{0}' requires [Patch] or [HarmonyPatch] on its containing class",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingTarget = new(
        "DISHARMONY0007", "Patch method requires a target attribute",
        "Patch method '{0}' requires [Target] or [Targets] on the method or its containing class",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingPatchType = new(
        "DISHARMONY0008", "Disharmony method attributes require a patch type",
        "Method '{0}' has a Disharmony attribute but no [Prefix] or [Postfix]; add the appropriate patch attribute or remove the unused Disharmony attribute",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultiplePatchTypes = new(
        "DISHARMONY0009", "Patch method has multiple patch type attributes",
        "Method '{0}' has multiple prefix/postfix attributes",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleInnerTargets = new(
        "DISHARMONY0010", "Patch method has multiple inner target attributes",
        "Method '{0}' has multiple inner target attributes",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingTargetType = new(
        "DISHARMONY0011", "Member selector has no declaring type",
        "Selector for patch '{0}' has no declaring type; supply a type or use a qualified member name such as Namespace.Type.Member",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NullInnerConstant = new(
        "DISHARMONY0012", "Inner constant cannot be null",
        "Patch '{0}' uses [InnerConstant] with null, which is unsupported",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateDiscoveryAttributes = new(
        "DISHARMONY0014", "Duplicate patch discovery attributes",
        "Class '{0}' has multiple {1} attributes",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingMemberName = new(
        "DISHARMONY0015", "Member selector requires a name",
        "Selector for patch '{0}' has no member name; supply a name or specify MemberType.Constructor to select a constructor",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleParameterBindings = new(
        "DISHARMONY0016", "Multiple parameter binding attributes", 
        "Parameter '{0}' has multiple binding attributes",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InnerBindingWithoutInnerPatch = new(
        "DISHARMONY0017", "Parameter binding requires an inner patch",
        "Parameter '{0}' uses __caller or Scope.Inner without an inner patch",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AlwaysRunResultBinding = new(
        "DISHARMONY0018", "AlwaysRun prefix cannot bind the result",
        "Parameter '{0}' binds the result in an AlwaysRun prefix, which is unsupported",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidExceptionBinding = new(
        "DISHARMONY0019", "Exception binding requires an AlwaysRun postfix",
        "Parameter '{0}' binds an exception outside an AlwaysRun postfix",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidDelegateBinding = new(
        "DISHARMONY0020", "Method binding requires a delegate value",
        "Parameter '{0}' binds a method; use a concrete delegate type such as Action or Func and remove any ref, in, or out modifier",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncompatibleBindingType = new(
        "DISHARMONY0021", "Incompatible parameter binding type",
        "Parameter '{0}' cannot bind a value of type '{1}'; use a compatible parameter type and ref/in/out modifier",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncompatibleStateTypes = new(
        "DISHARMONY0022", "Incompatible shared state types",
        "Parameter '{0}' shares state key '{1}' with a parameter of a different type",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConstantBindingUnavailable = new(
        "DISHARMONY0024", "Inner constant cannot supply this binding",
        "Parameter '{0}' requests an instance, argument, or field from [InnerConstant], which has none",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor VoidPrefixResultBinding = new(
        "DISHARMONY0025", "Prefix binding the result cannot skip the target",
        "Prefix binds the result through '{0}' but returns void; return bool and return false when supplying a replacement result to skip the target",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnknownSpecialParameter = new(
        "DISHARMONY0027", "Unknown special parameter name",
        "Parameter '{0}' starts with '__' but is not a recognized special name; correct the name or use an explicit binding attribute such as [Argument]",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateBinding = new(
        "DISHARMONY0028", "Patch binds the same value more than once",
        "Parameter '{0}' binds the same value as another parameter",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StateWithoutWriter = new(
        "DISHARMONY0029", "State has no writer",
        "State key '{0}' has no writer in this patch class",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StateWithoutReader = new(
        "DISHARMONY0030", "State has no reader",
        "State key '{0}' has no reader in this patch class",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor WrittenValueParameter = new(
        "DISHARMONY0031", "Patch writes to a parameter passed by value",
        "Writing to parameter '{0}' changes only the patch's local copy; declare it ref or out to update the bound value, or use a local variable for a temporary value",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AlwaysRunThrow = new(
        "DISHARMONY0032", "AlwaysRun patch explicitly throws",
        "AlwaysRun patch '{0}' explicitly throws; handle the failure without throwing so other AlwaysRun patches can execute",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    // DISHARMONY0033: Reserved for experimental flag for PatchOptions.Optimize

    public static readonly DiagnosticDescriptor PrefixResultIsRef = new(
        "DISHARMONY0034", "Prefix result is ref",
        "Prefix result binding is declared as ref; declare it as out instead",
        "Style", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PostfixArgumentIsOut = new(
        "DISHARMONY0035", "Postfix argument binding is out",
        "Postfix argument '{0}' is declared out; did you mean to write a prefix?",
        "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BindingRequiresValue = new(
        "DISHARMONY0036", "Binding requires a parameter passed by value",
        "Parameter '{0}' binds [Arguments] or [MemberInfo] by reference, which is unsupported",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConstantMemberUnavailable = new(
        "DISHARMONY0037", "Inner constant has no member metadata or base method",
        "Parameter '{0}' requests member metadata or a base method from [InnerConstant], which has neither",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncompatibleMemberInfoType = new(
        "DISHARMONY0038", "Incompatible member metadata type",
        "Parameter '{0}' cannot bind FieldInfo, MethodInfo, or ConstructorInfo",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterChecks);
    }

    private static void RegisterChecks(CompilationStartAnalysisContext start)
    {
        var state = new DiagnosticGenerator((CSharpCompilation)start.Compilation);

        if (!state.CanRun)
            return;

        start.RegisterOperationAction(state.AnalyzeAssignment,
            OperationKind.SimpleAssignment, OperationKind.CompoundAssignment, OperationKind.CoalesceAssignment,
            OperationKind.DeconstructionAssignment, OperationKind.Increment, OperationKind.Decrement, OperationKind.Argument);

        start.RegisterSymbolAction(state.AnalyzeClass, SymbolKind.NamedType);
        start.RegisterSymbolAction(state.AnalyzeMethod, SymbolKind.Method);
        start.RegisterOperationAction(state.AnalyzeThrow, OperationKind.Throw);
    }
}

internal enum ParameterKind
{
    Argument,
    Arguments,
    MemberInfo,
    Instance,
    Result,
    State,
    Field,
    BaseMethod,
    Method,
    Exception,
    Caller,
}
