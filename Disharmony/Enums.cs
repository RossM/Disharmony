using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Disharmony;

/// <summary>
///     Specifies whether a patch runs before or after its target operation.
/// </summary>
/// <seealso cref="PrefixAttribute" />
/// <seealso cref="PostfixAttribute" />
/// <seealso cref="Patch" />
public enum PatchKind
{
    /// <summary>
    ///     Runs before the target operation.
    /// </summary>
    Prefix,

    /// <summary>
    ///     Runs after the target operation.
    /// </summary>
    Postfix,
}

/// <summary>
///     Specifies optional patch behaviors that can be combined as flags.
/// </summary>
/// <remarks>
///     For an attributed patch, pass the desired flags to <see cref="PatchOptionsAttribute" />. When configuring a patch
///     in code, use <see cref="Patch.Options(PatchConfig, PatchOptions)" />.
///     <para>
///         See <see href="../usage-examples.md#alwaysrun-postfixes">handling an exception with an AlwaysRun postfix</see>
///         and <see href="../usage-examples.md#lambda-targets-and-inlining">inlining a patch</see> for worked examples.
///     </para>
/// </remarks>
[Flags]
public enum PatchOptions
{
    /// <summary>
    ///     Enables no additional behavior.
    /// </summary>
    Default = 0,

    /// <summary>
    ///     Runs the patch as part of each target instead of making a separate call to the patch method.
    /// </summary>
    Inline = 0x1,

    /// <summary>
    ///     Requests Disharmony's optional, experimental IL optimization pass.
    /// </summary>
    /// <remarks>
    ///     The optimization pass is disabled by default and has no public enablement API in the initial beta.
    ///     Setting this flag alone does not run the optimizer.
    /// </remarks>
    [Experimental("DISHARMONY0033")] Optimize = 0x2,

    /// <summary>
    ///     Requires that a patch always runs.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         On a prefix, this flag causes the prefix to always run before other prefixes. Prefixes with
    ///         this flag must return <see langword="void" /> and cannot access the target operation's return value.
    ///     </para>
    ///     <para>
    ///         On a postfix, this flag causes the postfix to run even if the method or another patch throws
    ///         an exception. The method may use <c>__exception</c> or <see cref="ExceptionAttribute" /> to
    ///         inspect or change the exception.
    ///     </para>
    ///     <para>
    ///         A patch method with this flag should never throw an exception itself. If it does, other prefixes
    ///         and postfixes with this flag will be skipped.
    ///     </para>
    /// </remarks>
    AlwaysRun = 0x4,

    /// <summary>
    ///     Disables selected safety checks that prevent Disharmony from generating invalid IL.
    /// </summary>
    /// <remarks>
    ///     In some cases, Disharmony may reject an operation that the caller knows is safe in a particular context. This
    ///     flag allows such operations by bypassing selected validation checks. Use it with care: an unsafe patch can
    ///     silently malfunction, produce IL that the JIT rejects, crash during execution, corrupt memory, or interfere
    ///     with other patches.
    /// </remarks>
    AllowUnsafe = 0x8,

    /// <summary>
    ///     Skips an inner patch when its selected operation is absent from an outer target.
    /// </summary>
    /// <remarks>
    ///     By default, a missing inner operation is reported as an error. Use this flag when the inner patch should be
    ///     optional for some outer targets.
    /// </remarks>
    AllowMissingInnerTarget = 0x10,

    /// <summary>
    ///     Logs the modified IL instructions.
    /// </summary>
    /// <remarks>
    ///     Output is written to the Harmony debug log.
    /// </remarks>
    Debug = 0x8000,
}

/// <summary>
///     Specifies whether an inner patch binds data from the inner member or its containing outer member.
/// </summary>
/// <remarks>
///     In an inner patch, the outer member is the method whose body is modified and the inner member is the
///     operation selected inside it. For example, <c>[Argument("buyer", Scope.Outer)]</c> binds the containing
///     method's buyer argument rather than the selected call's argument.
///     <para>
///         See <see href="../usage-examples.md#inner-calls-and-scope">choosing caller and inner-call values</see> for a worked example.
///     </para>
/// </remarks>
/// <seealso cref="InnerAttribute" />
/// <seealso cref="ArgumentAttribute" />
/// <seealso cref="FieldAttribute" />
/// <seealso cref="InstanceAttribute" />
public enum Scope
{
    /// <summary>
    ///     Uses the default scope for the binding.
    /// </summary>
    /// <remarks>
    ///     Ordinary patches use the outer member. Inner patches use the inner member, except that name-based parameter and
    ///     field lookups fall back to the outer member when the inner member has no match.
    /// </remarks>
    Any,

    /// <summary>
    ///     Uses data from the inner member.
    /// </summary>
    Inner,

    /// <summary>
    ///     Uses data from the outer member.
    /// </summary>
    Outer,
}

/// <summary>
///     Specifies the kind of member or member access selected by a target or inner-patch attribute.
/// </summary>
/// <seealso cref="TargetAttribute" />
/// <seealso cref="InnerAttribute" />
/// <seealso cref="MethodAttribute" />
[PublicAPI]
public enum MemberType
{
    /// <summary>
    ///     Matches methods, property getters, and field read accesses.
    /// </summary>
    Any,

    /// <summary>
    ///     Matches methods.
    /// </summary>
    Method,

    /// <summary>
    ///     Matches property getters and field read accesses.
    /// </summary>
    Getter,

    /// <summary>
    ///     Matches property setters and field write accesses.
    /// </summary>
    Setter,

    /// <summary>
    ///     Matches constructors.
    /// </summary>
    Constructor,
}
