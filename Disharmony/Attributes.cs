using JetBrains.Annotations;

namespace Disharmony;

/// <summary>
///     Represents a <see langword="ref" /> <typeparamref name="T" /> parameter in a target or inner-member signature.
/// </summary>
/// <typeparam name="T">The parameter's element type.</typeparam>
[PublicAPI]
public static class Ref<T>;

/// <summary>
///     Represents an <see langword="in" /> <typeparamref name="T" /> parameter in a target or inner-member signature.
/// </summary>
/// <typeparam name="T">The parameter's element type.</typeparam>
[PublicAPI]
public static class In<T>;

/// <summary>
///     Represents an <see langword="out" /> <typeparamref name="T" /> parameter in a target or inner-member signature.
/// </summary>
/// <typeparam name="T">The parameter's element type.</typeparam>
[PublicAPI]
public static class Out<T>;

/// <summary>
///     Marks a class as a patch container for assembly discovery and optionally supplies its default target type.
/// </summary>
/// <param name="type">
///     The default type that declares the outer target members, or <see langword="null" /> when target attributes identify
///     their declaring types individually.
/// </param>
/// <remarks>
///     <para>
///         <see cref="Patcher.PatchAll(Assembly)" /> and
///         <see cref="Patcher.PatchCategory" /> discover classes marked
///         with this attribute. Direct registration by type or method does not require it.
///     </para>
///     <para>
///         A type specified directly by <see cref="TargetAttribute" /> or <see cref="TargetsAttribute" /> takes
///         precedence.
///         Harmony's <see cref="HarmonyLib.HarmonyPatch" /> is also recognized for compatibility, but this attribute is
///         preferred for new patch classes. See <see cref="Patcher" /> for the complete authoring and targeting model.
///     </para>
/// </remarks>
[PublicAPI]
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public sealed class PatchAttribute(Type? type = null) : Attribute
{
    /// <summary>
    ///     Gets the default declaring type for outer targets, or <see langword="null" /> if none was supplied.
    /// </summary>
    public Type? Type { get; } = type;
}

/// <summary>
///     Assigns a patch container to a named category for category-specific assembly discovery.
/// </summary>
/// <param name="category">The category name used to select the patch class.</param>
/// <remarks>
///     <para>
///         <see cref="Patcher.PatchCategory" /> compares this name with
///         the requested category. <see cref="Patcher.PatchAll(Assembly)" /> ignores
///         categories.
///     </para>
///     <para>
///         This attribute does not make the class discoverable by itself. New patch containers should also use
///         <see cref="PatchAttribute" />. Existing Harmony metadata can instead supply
///         <see cref="HarmonyLib.HarmonyPatch" /> as the patch marker, and
///         <see cref="HarmonyLib.HarmonyPatchCategory" /> is recognized as an alternative category declaration.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CategoryAttribute(string category) : Attribute
{
    /// <summary>
    ///     Gets the category name used by <see cref="Patcher.PatchCategory" />.
    /// </summary>
    public string Category { get; } = category;
}

/// <summary>
///     Provides the base class for attributes that identify whether a patch method is a prefix or postfix.
/// </summary>
/// <param name="patchKind">The point relative to the target operation at which the patch runs.</param>
public abstract class PatchTypeAttribute(PatchKind patchKind) : Attribute
{
    /// <summary>
    ///     Gets the point relative to the target operation at which the patch runs.
    /// </summary>
    public PatchKind PatchKind { get; } = patchKind;
}

/// <summary>
///     Marks a patch method to run before each selected target operation.
/// </summary>
/// <remarks>
///     Without an inner-target attribute, the operation is the outer member selected by <see cref="TargetAttribute" />
///     or <see cref="TargetsAttribute" />. With <see cref="InnerAttribute" /> or <see cref="InnerConstantAttribute" />,
///     it is each matching operation inside the outer member. A prefix that returns <see langword="false" /> skips
///     that operation. See <see cref="Patcher" /> for the complete patch model.
/// </remarks>
[PublicAPI]
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public sealed class PrefixAttribute() : PatchTypeAttribute(PatchKind.Prefix);

/// <summary>
///     Marks a patch method to run after each selected target operation.
/// </summary>
/// <remarks>
///     Without an inner-target attribute, the operation is the outer member selected by <see cref="TargetAttribute" />
///     or <see cref="TargetsAttribute" />. With <see cref="InnerAttribute" /> or <see cref="InnerConstantAttribute" />,
///     it is each matching operation inside the outer member. Bind the operation's return value with
///     <see cref="ReturnValueAttribute" /> or the conventional parameter name <c>__result</c>; pass it by reference to
///     replace it. See <see cref="Patcher" /> for the complete patch model.
/// </remarks>
[PublicAPI]
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public sealed class PostfixAttribute() : PatchTypeAttribute(PatchKind.Postfix);

/// <summary>
///     Provides the base class for attributes that select an operation inside an outer patch target.
/// </summary>
public abstract class InnerAttributeBase : Attribute;

/// <summary>
///     Selects each matching member access or call inside the outer targets as the operation to patch.
/// </summary>
/// <param name="type">
///     The type that declares the inner member.
/// </param>
/// <param name="memberName">
///     The name of the inner member to match, or <see langword="null" /> when matching a constructor.
/// </param>
/// <param name="memberType">The kind of member access or call to match.</param>
/// <param name="parameterTypes">
///     The parameter types used to select an overload, or <see langword="null" /> to match without a parameter signature.
/// </param>
/// <param name="genericTypes">
///     The generic type arguments used to select a constructed generic method, or <see langword="null" /> when not
///     selecting one.
/// </param>
/// <remarks>
///     Combine this attribute with <see cref="PrefixAttribute" /> or <see cref="PostfixAttribute" /> and select the outer
///     members with <see cref="TargetAttribute" /> or <see cref="TargetsAttribute" />. See <see cref="TargetAttribute" />
///     for member-name syntax and overload selection, and <see cref="Scope" /> for parameter-binding scope.
/// </remarks>
[PublicAPI]
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public sealed class InnerAttribute(
    Type type,
    string? memberName = null,
    MemberType memberType = MemberType.Any,
    Type[]? parameterTypes = null,
    Type[]? genericTypes = null) : InnerAttributeBase
{
    /// <summary>
    ///     Selects each access or call to the named inner member.
    /// </summary>
    /// <param name="type">
    ///     The type that declares the inner member.
    /// </param>
    /// <param name="memberName">The name of the inner member to match.</param>
    public InnerAttribute(Type type, string? memberName) : this(type, memberName, MemberType.Any) { }

    /// <summary>
    ///     Selects each call to the specified overload of the named inner member.
    /// </summary>
    /// <param name="type">
    ///     The type that declares the inner member.
    /// </param>
    /// <param name="memberName">The name of the inner member to match.</param>
    /// <param name="parameterTypes">The parameter types that identify the overload.</param>
    public InnerAttribute(Type type, string? memberName, params Type[] parameterTypes) : this(type, memberName, MemberType.Any,
        parameterTypes) { }

    /// <summary>
    ///     Gets the type that declares the inner member.
    /// </summary>
    public Type Type { get; } = type;

    /// <summary>
    ///     Gets the name of the inner member, or <see langword="null" /> when selecting a constructor.
    /// </summary>
    public string? MemberName { get; } = memberName;

    /// <summary>
    ///     Gets the kind of member access or call to match.
    /// </summary>
    public MemberType MemberType { get; } = memberType;

    /// <summary>
    ///     Gets the parameter types used to select an overload, or <see langword="null" /> when the selection does not
    ///     include a parameter signature.
    /// </summary>
    public Type[]? ParameterTypes { get; } = parameterTypes;

    /// <summary>
    ///     Gets the generic type arguments used to select a constructed generic method, or <see langword="null" /> when
    ///     the selection does not include generic type arguments.
    /// </summary>
    public Type[]? GenericTypes { get; } = genericTypes;
}

/// <summary>
///     Selects each occurrence of the specified constant inside the outer targets as the operation to patch.
/// </summary>
/// <remarks>
///     Combine this attribute with <see cref="PrefixAttribute" /> or <see cref="PostfixAttribute" />. Bind the constant
///     value with <see cref="ReturnValueAttribute" /> or the conventional parameter name <c>__result</c>; pass it by
///     reference to replace it. Select outer members with <see cref="TargetAttribute" /> or
///     <see cref="TargetsAttribute" />. See <see cref="Patcher" /> for the complete patch model.
/// </remarks>
[PublicAPI]
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public sealed class InnerConstantAttribute : InnerAttributeBase
{
    /// <summary>
    ///     Selects each matching 32-bit integer constant in IL.
    /// </summary>
    /// <remarks>
    ///     IL provides only 32-bit and 64-bit integer constant forms, so constants of smaller integer types, including
    ///     <see langword="bool" />, are emitted as 32-bit values. <see langword="true" /> is represented as 1, and
    ///     <see langword="false" /> is represented as 0. Exercise caution when patching 0 or 1 because they can match
    ///     <see langword="bool" /> values introduced by the compiler.
    /// </remarks>
    /// <param name="value">The constant value to match.</param>
    public InnerConstantAttribute(int value)
    {
        Value = value;
    }

    /// <summary>
    ///     Selects each matching 64-bit integer constant.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    public InnerConstantAttribute(long value)
    {
        Value = value;
    }

    /// <summary>
    ///     Selects each matching single-precision floating-point constant.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    public InnerConstantAttribute(float value)
    {
        Value = value;
    }

    /// <summary>
    ///     Selects each matching double-precision floating-point constant.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    public InnerConstantAttribute(double value)
    {
        Value = value;
    }

    /// <summary>
    ///     Selects each matching string constant.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    public InnerConstantAttribute(string value)
    {
        Value = value;
    }

    /// <summary>
    ///     Gets the constant value to match.
    /// </summary>
    public object Value { get; }
}

/// <summary>
///     Enables optional behaviors for the attributed patch method or patch class.
/// </summary>
/// <param name="options">The behaviors to enable. Multiple <see cref="PatchOptions" /> flags can be combined.</param>
/// <remarks>
///     Apply this attribute to a patch class to provide options for every patch method declared by the class. An attribute
///     on an individual patch method replaces the class-level options for that method. Programmatic patches instead use
///     <see cref="Patch.Options(PatchConfig, PatchOptions)" />.
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class PatchOptionsAttribute(PatchOptions options) : Attribute
{
    /// <summary>
    ///     Gets the optional patch behaviors to enable.
    /// </summary>
    public PatchOptions Options { get; } = options;
}

/// <summary>
///     Sets a patch's execution priority.
/// </summary>
/// <param name="priority">The priority value. See <see cref="PatchPriority" /> for standard values.</param>
/// <remarks>
///     Higher-priority <see cref="PrefixAttribute">prefixes</see> run earlier. Higher-priority patches of other types run
///     later.
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class PriorityAttribute(int priority) : Attribute
{
    /// <summary>
    ///     Gets the patch priority.
    /// </summary>
    public int Priority { get; } = priority;
}

/// <summary>
///     Provides standard values for ordering patches.
/// </summary>
/// <remarks>
///     Higher-priority <see cref="PrefixAttribute">prefixes</see> run earlier. Higher-priority patches of other types run
///     later. Use <see cref="Default" /> when no explicit ordering is required.
/// </remarks>
[PublicAPI]
public static class PatchPriority
{
    /// <summary>
    ///     The standard very-low priority.
    /// </summary>
    public const int VeryLow = 0;

    /// <summary>
    ///     The standard low priority.
    /// </summary>
    public const int Low = 500;

    /// <summary>
    ///     The default priority assigned when none is specified.
    /// </summary>
    public const int Default = 1000;

    /// <summary>
    ///     The standard high priority.
    /// </summary>
    public const int High = 1500;

    /// <summary>
    ///     The standard very-high priority.
    /// </summary>
    public const int VeryHigh = 2000;
}

/// <summary>
///     Selects exactly one outer method, constructor, or property accessor for the attributed patch method or methods.
/// </summary>
/// <param name="type">
///     The type that declares the target, or <see langword="null" /> to use the default from a containing
///     <see cref="PatchAttribute" /> or Harmony patch attribute, or to resolve it from <paramref name="methodName" />.
/// </param>
/// <param name="methodName">
///     The name of the target member, or <see langword="null" /> when targeting a constructor.
/// </param>
/// <param name="memberType">The kind of member or accessor to target.</param>
/// <param name="parameterTypes">
///     The parameter types used to select an overload, or <see langword="null" /> to omit parameter filtering.
/// </param>
/// <remarks>
///     <para>
///         When applied to a class, this target applies to every patch method declared by the class. Method-level targets
///         are added to class-level targets.
///     </para>
///     <para>
///         The selection must resolve to exactly one member. Use <see cref="TargetsAttribute" /> to patch every match.
///         Constructed generic methods can be identified during lookup but are not currently supported as outer targets.
///     </para>
///     <para>
///         Use <c>Namespace.Type.Member</c> to include the declaring type in the member name. A type resolved
///         from the name overrides the type supplied by this attribute or the containing patch class.
///     </para>
///     <para>
///         For Harmony compatibility, <c>Namespace.Type:Member</c> is also accepted. The type named before the colon
///         overrides any type supplied by patch attributes.
///     </para>
///     <para>
///         Use <c>Method.LocalFunction</c> to select a local function or <c>Method.*</c> to select lambdas within a method.
///     </para>
///     <para>
///         To disambiguate nested types, use <c>Namespace.Outer+Inner:Member</c>. If multiple assemblies define
///         the same full type name, use <c>Namespace.Outer+Inner, AssemblyName:Member</c> or a complete
///         <see cref="System.Type.AssemblyQualifiedName" /> before the colon.
///     </para>
///     <para>
///         Full type names take precedence over abbreviated names and generally resolve faster.
///     </para>
///     <para>
///         A <see langword="null" /> parameter signature omits overload filtering; an empty array selects parameterless overloads.
///         Use <see cref="Ref{T}" />, <see cref="In{T}" />, and <see cref="Out{T}" /> in the signature for
///         by-reference parameters. To select a constructor, set <paramref name="memberType" /> to
///         <see cref="MemberType.Constructor" /> and leave <paramref name="methodName" /> <see langword="null" />.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class TargetAttribute(
    Type? type,
    string? methodName = null,
    MemberType memberType = MemberType.Any,
    Type[]? parameterTypes = null)
    : Attribute
{
    /// <summary>
    ///     Applies the patch to a named member on the specified type.
    /// </summary>
    /// <param name="type">
    ///     The type that declares the target member, or <see langword="null" /> to resolve it from
    ///     <paramref name="methodName" /> or containing patch metadata.
    /// </param>
    /// <param name="methodName">The name of the target member.</param>
    public TargetAttribute(Type? type, string? methodName)
        : this(type, methodName, MemberType.Any) { }

    /// <summary>
    ///     Applies the patch to a specific overload on the specified type.
    /// </summary>
    /// <param name="type">
    ///     The type that declares the target member, or <see langword="null" /> to resolve it from
    ///     <paramref name="methodName" /> or containing patch metadata.
    /// </param>
    /// <param name="methodName">The name of the target member.</param>
    /// <param name="parameterTypes">The parameter types that identify the overload.</param>
    public TargetAttribute(Type? type, string? methodName, params Type[] parameterTypes)
        : this(type, methodName, MemberType.Any, parameterTypes) { }

    /// <summary>
    ///     Applies the patch to a member whose declaring type is resolved from the member name or containing patch
    ///     metadata.
    /// </summary>
    /// <param name="methodName">
    ///     The name of the target member, or <see langword="null" /> when targeting a constructor.
    /// </param>
    /// <param name="memberType">The kind of member or accessor to target.</param>
    /// <param name="parameterTypes">
    ///     The parameter types used to select an overload, or <see langword="null" /> to match without a parameter signature.
    /// </param>
    public TargetAttribute(
        string? methodName = null,
        MemberType memberType = MemberType.Any,
        Type[]? parameterTypes = null)
        : this(null, methodName, memberType, parameterTypes) { }

    /// <summary>
    ///     Applies the patch to a member whose declaring type is resolved from the member name or containing patch
    ///     metadata.
    /// </summary>
    /// <param name="methodName">The name of the target member.</param>
    public TargetAttribute(string methodName)
        : this(null, methodName, MemberType.Any) { }

    /// <summary>
    ///     Applies the patch to a specific overload whose declaring type is resolved from the member name or containing
    ///     patch metadata.
    /// </summary>
    /// <param name="methodName">The name of the target member.</param>
    /// <param name="parameterTypes">The parameter types that identify the overload.</param>
    public TargetAttribute(string methodName, params Type[] parameterTypes)
        : this(null, methodName, MemberType.Any, parameterTypes) { }

    /// <summary>
    ///     Gets the type that declares the target members, or <see langword="null" /> when the type is supplied by
    ///     containing patch metadata or resolved from <see cref="MethodName" />.
    /// </summary>
    public Type? Type { get; } = type;

    /// <summary>
    ///     Gets the target member name, or <see langword="null" /> when selecting a constructor.
    /// </summary>
    public string? MethodName { get; } = methodName;

    /// <summary>
    ///     Gets the kind of member or accessor to select.
    /// </summary>
    public MemberType MemberType { get; } = memberType;

    /// <summary>
    ///     Gets the parameter types used to filter overloads, or <see langword="null" /> when the selection does not
    ///     include a parameter signature.
    /// </summary>
    public Type[]? ParameterTypes { get; } = parameterTypes;
}

/// <summary>
///     Selects every outer method, constructor, or property accessor matching the supplied criteria for the attributed
///     patch method or methods.
/// </summary>
/// <param name="type">
///     The type that declares the targets, or <see langword="null" /> to use the default from a containing
///     <see cref="PatchAttribute" /> or Harmony patch attribute, or to resolve it from <paramref name="methodName" />.
/// </param>
/// <param name="methodName">
///     The name shared by the target members, or <see langword="null" /> when targeting constructors.
/// </param>
/// <param name="memberType">The kind of members or accessors to target.</param>
/// <param name="parameterTypes">
///     The parameter types used to filter overloads, or <see langword="null" /> to omit parameter filtering.
/// </param>
/// <remarks>
///     <para>
///         When applied to a class, these targets apply to every patch method declared by the class. Method-level targets
///         are added to class-level targets.
///     </para>
///     <para>
///         Every match is patched; use <see cref="TargetAttribute" /> when the selection must resolve to exactly one
///         member. Constructed generic methods can be identified during lookup but are not currently supported as outer
///         targets.
///     </para>
///     <para>
///         See <see cref="TargetAttribute" /> for member-name syntax, overload selection, and constructor selection.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class TargetsAttribute(
    Type? type,
    string? methodName = null,
    MemberType memberType = MemberType.Any,
    Type[]? parameterTypes = null)
    : TargetAttribute(type, methodName, memberType, parameterTypes)
{
    /// <summary>
    ///     Applies the patch to every member with the specified name on the specified type.
    /// </summary>
    /// <param name="type">
    ///     The type that declares the target members, or <see langword="null" /> to resolve it from
    ///     <paramref name="methodName" /> or containing patch metadata.
    /// </param>
    /// <param name="methodName">The name shared by the target members.</param>
    public TargetsAttribute(Type? type, string? methodName)
        : this(type, methodName, MemberType.Any) { }

    /// <summary>
    ///     Applies the patch to every matching overload on the specified type.
    /// </summary>
    /// <param name="type">
    ///     The type that declares the target members, or <see langword="null" /> to resolve it from
    ///     <paramref name="methodName" /> or containing patch metadata.
    /// </param>
    /// <param name="methodName">The name shared by the target members.</param>
    /// <param name="parameterTypes">The parameter types used to filter the overloads.</param>
    public TargetsAttribute(Type? type, string? methodName, params Type[] parameterTypes)
        : this(type, methodName, MemberType.Any, parameterTypes) { }

    /// <summary>
    ///     Applies the patch to every matching member whose declaring type is resolved from the member name or containing
    ///     patch metadata.
    /// </summary>
    /// <param name="methodName">
    ///     The name shared by the target members, or <see langword="null" /> when targeting constructors.
    /// </param>
    /// <param name="memberType">The kind of members or accessors to target.</param>
    /// <param name="parameterTypes">
    ///     The parameter types used to filter overloads, or <see langword="null" /> to match without a parameter signature.
    /// </param>
    public TargetsAttribute(
        string? methodName = null,
        MemberType memberType = MemberType.Any,
        Type[]? parameterTypes = null)
        : this(null, methodName, memberType, parameterTypes) { }

    /// <summary>
    ///     Applies the patch to every matching member whose declaring type is resolved from the member name or containing
    ///     patch metadata.
    /// </summary>
    /// <param name="methodName">The name shared by the target members.</param>
    public TargetsAttribute(string methodName)
        : this(null, methodName, MemberType.Any) { }

    /// <summary>
    ///     Applies the patch to every matching overload whose declaring type is resolved from the member name or containing
    ///     patch metadata.
    /// </summary>
    /// <param name="methodName">The name shared by the target members.</param>
    /// <param name="parameterTypes">The parameter types used to filter the overloads.</param>
    public TargetsAttribute(string methodName, params Type[] parameterTypes)
        : this(null, methodName, MemberType.Any, parameterTypes) { }
}

/// <summary>
///     Provides the base class for attributes that bind a patch-method parameter to data from the patched operation.
/// </summary>
/// <param name="scope">The inner or outer operation from which to obtain the bound value.</param>
public abstract class ParameterBindingAttribute(Scope scope) : Attribute
{
    /// <summary>
    ///     Gets the operation from which to obtain the bound value.
    /// </summary>
    public Scope Scope { get; } = scope;
}

/// <summary>
///     Binds a patch parameter to a parameter of either the outer or inner member. The source parameter can be selected by
///     name or zero-based index, and <see cref="Scope" /> selects the member for an inner patch.
/// </summary>
/// <remarks>
///     <para>
///         Patch parameters bind by name without this attribute. Use it when the patch parameter has a different name, when
///         positional binding is more stable, or when an inner patch must select a specific <see cref="Scope" />.
///     </para>
///     <para>
///         For inner <see cref="MemberType.Setter" /> patches on a field, the new value being assigned is available as
///         a parameter named <see langword="value" />.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ArgumentAttribute : ParameterBindingAttribute
{
    /// <summary>
    ///     Binds to the source parameter having the same name as the attributed patch parameter.
    /// </summary>
    /// <param name="scope">
    ///     The member whose parameter is bound in an inner patch. The default, <see cref="Scope.Any" />, searches the inner
    ///     member first and then the outer member.
    /// </param>
    public ArgumentAttribute(Scope scope = Scope.Any) : base(scope) { }

    /// <summary>
    ///     Binds to a source parameter by name.
    /// </summary>
    /// <param name="name">
    ///     The source parameter name, or <see langword="null" /> to use the attributed patch parameter's name.
    /// </param>
    /// <param name="scope">
    ///     The member whose parameter is bound in an inner patch. The default, <see cref="Scope.Any" />, searches the inner
    ///     member first and then the outer member.
    /// </param>
    public ArgumentAttribute(string? name, Scope scope = Scope.Any) : base(scope)
    {
        Name = name;
    }

    /// <summary>
    ///     Binds to a source parameter by position.
    /// </summary>
    /// <param name="index">The zero-based index of the source parameter, excluding the instance argument.</param>
    /// <param name="scope">
    ///     The member whose parameter is bound in an inner patch. The default, <see cref="Scope.Any" />, uses the inner
    ///     member for an inner patch and the outer member otherwise.
    /// </param>
    public ArgumentAttribute(int index, Scope scope = Scope.Any) : base(scope)
    {
        Index = index;
    }

    /// <summary>
    ///     Gets the zero-based source parameter index, or <see langword="null" /> when binding by name.
    /// </summary>
    public int? Index { get; } = null;

    /// <summary>
    ///     Gets the source parameter name, or <see langword="null" /> when using the patch parameter's name or an index.
    /// </summary>
    public string? Name { get; } = null;
}

/// <summary>
///     Binds a patch parameter to an <see cref="object" /> array containing the argument values of the outer or inner
///     member, excluding the instance argument.
/// </summary>
/// <param name="scope">
///     The member whose arguments are bound. The default, <see cref="Scope.Any" />, uses the inner member for an
///     inner patch and the outer member otherwise.
/// </param>
/// <remarks>
///     <para>
///         Assigning array elements does not change the corresponding arguments. Value-type arguments are boxed.
///         The patch parameter cannot be passed by reference.
///     </para>
///     <para>
///         A parameter named <c>__args</c> with no binding attribute is treated as if it has this attribute.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ArgumentsAttribute(Scope scope = Scope.Any) : ParameterBindingAttribute(scope);

/// <summary>
///     Binds a patch parameter to the instance on which either the outer or inner member is invoked.
/// </summary>
/// <param name="scope">
///     The member whose instance is bound in an inner patch. The default, <see cref="Scope.Any" />, uses the inner member
///     for an inner patch and the outer member otherwise.
/// </param>
/// <remarks>
///     <para>
///         A parameter named <c>__instance</c> with no binding attribute is treated as if it has this attribute with
///         <see cref="Scope.Any" />.
///     </para>
///     <para>
///         For inner patches, a parameter named <c>__caller</c> with no binding attribute is treated as if it has
///         this attribute with <see cref="Scope.Outer" />.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class InstanceAttribute(Scope scope = Scope.Any) : ParameterBindingAttribute(scope);

/// <summary>
///     Binds a patch parameter to the outer member's return value for an ordinary patch or the inner member's return value
///     for an inner patch.
/// </summary>
/// <remarks>
///     <para>
///         Pass the parameter by reference to replace the bound return value. A parameter named <c>__result</c>
///         with no binding attribute is treated as if it has this attribute.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ReturnValueAttribute() : ParameterBindingAttribute(Scope.Any);

/// <summary>
///     Binds a patch parameter to temporary state for the current invocation of the outer member.
/// </summary>
/// <param name="key">
///     The key used to identify the shared state, or <see langword="null" /> to use the attributed patch parameter's name.
/// </param>
/// <remarks>
///     <para>
///         State is not supported when an inner patch targets an iterator state-machine method.
///     </para>
///     <para>
///         Patches only share state with other patches applied in the same call to a patching method on
///         <see cref="Patcher" />.
///         Patches applied with <see cref="Patcher.PatchAll(Assembly)" /> or <see cref="Patcher.PatchCategory" />
///         additionally require that the patch methods be defined in the same class to share state.
///     </para>
///     <para>
///         A parameter named <c>__state</c> with no binding attribute is treated as if it has this attribute.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class StateAttribute(string? key) : ParameterBindingAttribute(Scope.Outer)
{
    /// <summary>
    ///     Binds temporary state using the attributed patch parameter's name as the key.
    /// </summary>
    public StateAttribute() : this(null) { }

    /// <summary>
    ///     Gets the state key, or <see langword="null" /> when the patch parameter's name is used.
    /// </summary>
    public string? Key { get; } = key;
}

/// <summary>
///     Binds a patch parameter to an instance or static field. For instance fields, <see cref="Scope" /> selects the
///     instance from the outer or inner member.
/// </summary>
/// <param name="type">
///     The type on which to look up the field, or <see langword="null" /> to use the type associated with the selected
///     scope. A type-qualified field name overrides this type.
/// </param>
/// <param name="name">
///     The optionally type-qualified field name, or <see langword="null" /> to use the attributed patch parameter's name
///     with any leading <c>___</c> prefix removed.
/// </param>
/// <param name="scope">
///     The member supplying the instance and default field lookup type. The default, <see cref="Scope.Any" />, searches the inner
///     member first and then the outer member.
/// </param>
/// <remarks>
///     <para>
///         A parameter starting with <c>___</c> (three underscores) with no binding attribute is treated as if it has
///         this attribute, with the field name starting after the first three underscores.
///     </para>
///     <para>
///         If used on an auto-property, this will bind the property's backing field. To bind other properties
///         use <see cref="MethodAttribute" /> with <see cref="MemberType.Getter" /> or <see cref="MemberType.Setter" />.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FieldAttribute(Type? type, string? name, Scope scope = Scope.Any) : ParameterBindingAttribute(scope)
{
    /// <summary>
    ///     Binds to a field by name, using the selected scope for an unqualified name.
    /// </summary>
    /// <param name="name">
    ///     The optionally type-qualified field name, or <see langword="null" /> to use the attributed patch parameter's
    ///     name with any leading <c>___</c> prefix removed.
    /// </param>
    /// <param name="scope">
    ///     The member supplying the instance and default lookup type. The default, <see cref="Scope.Any" />, searches
    ///     the inner member first and then the outer member.
    /// </param>
    public FieldAttribute(string? name, Scope scope = Scope.Any) : this(null, name, scope) { }

    /// <summary>
    ///     Binds to the field named by the attributed patch parameter, with any leading <c>___</c> prefix removed.
    /// </summary>
    /// <param name="scope">
    ///     The member supplying the instance and default field lookup type. The default, <see cref="Scope.Any" />, searches the
    ///     inner member first and then the outer member.
    /// </param>
    public FieldAttribute(Scope scope = Scope.Any) : this(null, scope) { }

    /// <summary>
    ///     Gets the optionally type-qualified field name, or <see langword="null" /> when the patch parameter's name is used
    ///     with any leading <c>___</c> prefix removed.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    ///     Gets the field lookup type, or <see langword="null" /> to use the type associated with the selected scope.
    ///     A type-qualified field name overrides this type.
    /// </summary>
    public Type? Type { get; } = type;
}

/// <summary>
///     Binds a patch parameter to a delegate that invokes the nearest base-class implementation of the instance
///     method, allowing the patch to call that implementation directly as if using <see langword="base" />.
/// </summary>
/// <param name="scope">
///     The instance to use. The default, <see cref="Scope.Any" />, uses the inner instance for an
///     inner patch and the outer instance otherwise.
/// </param>
/// <remarks>
///     <para>
///         The delegate is bound to the instance from the selected scope and invokes the nearest implementation in
///         that instance type's base-class hierarchy. The patch parameter must be a delegate whose parameters and
///         return type match the method. Static methods do not have a base-method binding.
///     </para>
///     <para>
///         A parameter named <c>__base</c> with no binding attribute is treated as if it has this attribute.
///     </para>
///     <para>
///         When patching an inner virtual call, be aware that the C# compiler may generate a call to the base method
///         even when the declared type is a derived class. In that case, the inner target must be the base
///         method, and base-method binding is not available. Instead use <see cref="MethodAttribute" /> with
///         <see cref="MethodAttribute.VirtualCall" /> set to <see langword="false" />.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class BaseMethodAttribute(Scope scope = Scope.Any) : ParameterBindingAttribute(scope);

/// <summary>
///     Binds a patch parameter to a delegate that invokes an instance or static method.
/// </summary>
/// <param name="type">
///     The type on which to look up the method, or <see langword="null" /> to use the type associated with the selected
///     scope. A type-qualified method name overrides this type.
/// </param>
/// <param name="name">
///     The method name in <c>"Method"</c>, <c>"Type.Method"</c>, or <c>"Namespace.Type.Method"</c> form,
///     or <see langword="null" /> to use the attributed patch parameter's name.
/// </param>
/// <param name="scope">
///     The instance used for instance methods and the default type for unqualified method names.
///     The default, <see cref="Scope.Any" />, uses the inner instance for an inner patch and the outer instance otherwise.
/// </param>
/// <param name="virtualCall">
///     A value indicating whether to use a virtual call if the method is virtual.
/// </param>
/// <remarks>
///     <para>
///         For an instance method, the delegate is bound to the instance from the selected scope. The method must be
///         declared on that instance's type or one of its base types. Static methods can be declared on any type.
///         Supply <paramref name="type" /> or a type-qualified <paramref name="name" /> to choose the lookup type;
///         this does not change the instance to which the delegate is bound.
///     </para>
///     <para>
///         If the selected method is overloaded, the type of the delegate parameter is used to select the overload.
///         The delegate's parameters and return type must match the selected method.
///     </para>
///     <para>
///         This binding can be used to invoke an otherwise-inaccessible method, or to call a base class method on a
///         derived class instance by setting <paramref name="virtualCall" /> to <see langword="false" />.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class MethodAttribute(
    Type? type,
    string? name = null,
    Scope scope = Scope.Any,
    MemberType memberType = MemberType.Method,
    bool virtualCall = true) : ParameterBindingAttribute(scope)
{
    /// <summary>
    ///     Binds to a delegate for a method selected by name and the patch parameter's delegate signature.
    /// </summary>
    /// <param name="name">
    ///     The optionally type-qualified method name, or <see langword="null" /> to use the attributed patch parameter's name.
    /// </param>
    /// <param name="scope">
    ///     The instance used for instance methods and the default type for unqualified method names.
    ///     The default, <see cref="Scope.Any" />, uses the inner instance for an inner patch and the outer instance otherwise.
    /// </param>
    /// <param name="memberType">The kind of member or accessor to target.</param>
    /// <param name="virtualCall">Whether to dispatch virtual methods to the most-derived override.</param>
    public MethodAttribute(string? name, Scope scope = Scope.Any, MemberType memberType = MemberType.Method, bool virtualCall = true) : this(null, name, scope, memberType, virtualCall) { }

    /// <summary>
    ///     Binds to the method having the same name as the attributed patch parameter.
    /// </summary>
    /// <param name="scope">
    ///     The instance used for instance methods and the default type for unqualified method names.
    ///     The default, <see cref="Scope.Any" />, uses the inner instance for an inner patch and the outer instance otherwise.
    /// </param>
    /// <param name="memberType">The kind of member or accessor to target.</param>
    /// <param name="virtualCall">A value indicating whether to use a virtual call if the method is virtual.</param>
    public MethodAttribute(Scope scope = Scope.Any, MemberType memberType = MemberType.Method, bool virtualCall = true) : this(null, null, scope, memberType, virtualCall) { }

    /// <summary>
    ///     Gets the optionally type-qualified method name, or <see langword="null" /> when the patch parameter's name is used.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    ///     Gets a value indicating whether to use a virtual call if the method is virtual.
    /// </summary>
    /// <remarks>
    ///     If this is false, the call will be to the exact declared method, bypassing any overrides.
    ///     If this is true, the call will be dispatched to the most-derived override of the method.
    /// </remarks>
    public bool VirtualCall { get; } = virtualCall;

    /// <summary>
    ///     Gets the method lookup type, or <see langword="null" /> to use the type associated with the selected scope.
    ///     A type-qualified method name overrides this type.
    /// </summary>
    public Type? Type { get; } = type;

    public MemberType MemberType { get; } = memberType;
}

/// <summary>
///     Binds a patch parameter to the exception thrown by the patched operation.
/// </summary>
/// <remarks>
///     <para>
///         If no exception is thrown, the value is <see langword="null" />. A <see langword="ref" /> parameter can
///         replace the exception or set it to <see langword="null" /> to suppress it.
///     </para>
///     <para>
///         This is only valid for <see cref="PostfixAttribute">postfixes</see> with the
///         <see cref="PatchOptions.AlwaysRun" />
///         option set.
///     </para>
///     <para>
///         A parameter named <c>__exception</c> with no binding attribute is treated as if it has this attribute.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ExceptionAttribute() : ParameterBindingAttribute(Scope.Any);

/// <summary>
///     Binds a patch parameter to the <see cref="MemberInfo" /> of the target member.
/// </summary>
/// <param name="scope">
///     The member whose metadata is bound. The default is <see cref="Scope.Outer" />, including for inner patches.
///     <see cref="Scope.Any" /> selects the inner member for an inner patch and the outer member otherwise.
/// </param>
/// <remarks>
///     <para>
///         This enables a single patch applied to multiple targets with <see cref="TargetsAttribute" /> to
///         determine which specific method is being patched.
///     </para>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class MemberInfoAttribute(Scope scope = Scope.Outer) : ParameterBindingAttribute(scope);
