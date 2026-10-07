using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Disharmony;

/// <summary>
///     Specifies whether a patch runs before or after its target operation.
/// </summary>
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
    ///     The optimization pass must be enabled separately.
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
    ///     Skips the patch if the target operation is not found.
    /// </summary>
    /// <remarks>
    ///     Ordinarily the rules engine will report an error if a patch's inner target operation is not found.
    ///     This flag causes the patch to instead be silently skipped.
    /// </remarks>
    AllowMissingInnerTarget = 0x10,

    /// <summary>
    ///     Logs the modified IL and, when available, the generated Mono JIT assembly.
    /// </summary>
    /// <remarks>
    ///     Output is written to the Harmony debug log.
    /// </remarks>
    Debug = 0x8000,
}

/// <summary>
///     Specifies whether an inner patch binds data from the inner member or its containing outer member.
/// </summary>
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
