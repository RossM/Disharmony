using JetBrains.Annotations;

namespace Disharmony;

/// <summary>
///     Describes a programmatically configured patch.
/// </summary>
/// <remarks>
///     Build a configuration with <see cref="Patch" />, then apply it with
///     <see cref="Patcher.Patch(PatchConfig)" /> or another programmatic patching overload. Builder operations return a
///     new configuration and leave the original unchanged.
/// </remarks>
[PublicAPI]
public record PatchConfig
{
    /// <summary>
    ///     Gets the outer method or constructor to patch, or <see langword="null" /> if none has been selected.
    /// </summary>
    public MethodBase? TargetMethod => (Target as MethodBaseInvocation)?.MethodBase;

    /// <summary>
    ///     Gets the method or constructor selected as the inner target, or <see langword="null" /> if the inner target is
    ///     a field, constant, or has not been selected.
    /// </summary>
    public MethodBase? InnerTargetMethod => (InnerTarget as MethodBaseInvocation)?.MethodBase;

    /// <summary>
    ///     Gets or initializes when the patch runs relative to the selected operation, or <see langword="null" /> if unset.
    /// </summary>
    public PatchKind? Kind { get; init; } = null;
    internal Invocation Target { get; init; } = EmptyInvocation.Instance;
    internal Invocation InnerTarget { get; init; } = EmptyInvocation.Instance;

    /// <summary>
    ///     Gets or initializes the static patch method, or <see langword="null" /> if none has been selected.
    /// </summary>
    public MethodInfo? PatchMethod { get; init; } = null;

    /// <summary>
    ///     Gets or initializes the optional patch behaviors. The default is <see cref="PatchOptions.Default" />.
    /// </summary>
    public PatchOptions Options { get; init; } = PatchOptions.Default;

    /// <summary>
    ///     Gets or initializes the execution priority. The default is <see cref="PatchPriority.Default" />.
    /// </summary>
    public int Priority { get; init; } = PatchPriority.Default;
}

/// <summary>
///     Identifies the patches added by one call to <see cref="Patcher" />.
/// </summary>
/// <remarks>
///     Retain this handle and pass it to <see cref="Patcher.Unpatch" /> to remove exactly that group of patches.
/// </remarks>
[PublicAPI]
public class PatchHandle
{
    private static int _nextId = 0;

    internal readonly int id;

    internal PatchHandle()
    {
        id = _nextId++;
    }
}

/// <summary>
///     Provides a fluent builder for <see cref="PatchConfig" /> instances.
/// </summary>
/// <remarks>
///     <para>
///         Start with any member, then continue configuring the result with the extension members on this class. A
///         complete configuration identifies a <see cref="Prefix" /> or <see cref="Postfix" />, an outer target through
///         <see cref="Of(MethodBase)" />, and a patch method through <see cref="With(MethodInfo)" />. Use an
///         <see cref="Inner(MethodBase)" />, <c>InnerGet</c>, <c>InnerSet</c>, or <c>InnerConstant</c> member when the
///         patch should surround a matching operation inside the outer target.
///     </para>
/// </remarks>
/// <example>
///     <code>
///     PatchHandle handle = Patcher.Patch(
///         Patch.Prefix
///             .Of(targetMethod)
///             .With(patchMethod));
///     </code>
/// </example>
[PublicAPI]
public static class Patch
{
    /// <summary>
    ///     Gets a new configuration for a patch that runs before its selected operation.
    /// </summary>
    public static PatchConfig Prefix => new PatchConfig().Prefix;

    /// <summary>
    ///     Gets a new configuration for a patch that runs after its selected operation.
    /// </summary>
    public static PatchConfig Postfix => new PatchConfig().Postfix;

    /// <summary>
    ///     Starts a configuration targeting the specified outer method or constructor.
    /// </summary>
    /// <param name="method">The outer method or constructor to patch.</param>
    /// <returns>A configuration whose outer target is <paramref name="method" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="method" /> is neither a <see cref="MethodInfo" /> nor a <see cref="ConstructorInfo" />.
    /// </exception>
    public static PatchConfig Of(MethodBase method) => new PatchConfig().Of(method);

    /// <summary>
    ///     Starts a configuration targeting calls to the specified method or constructor inside an outer target.
    /// </summary>
    /// <param name="member">The inner method or constructor to match.</param>
    /// <returns>A configuration whose inner target is <paramref name="member" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="member" /> is neither a <see cref="MethodInfo" /> nor a <see cref="ConstructorInfo" />.
    /// </exception>
    public static PatchConfig Inner(MethodBase member) => new PatchConfig().Inner(member);

    /// <summary>
    ///     Starts a configuration targeting reads of the specified property inside an outer target.
    /// </summary>
    /// <param name="member">The property whose getter calls are matched. The property must have a getter.</param>
    /// <returns>A configuration whose inner target is the property's getter.</returns>
    public static PatchConfig InnerGet(PropertyInfo member) => new PatchConfig().InnerGet(member);

    /// <summary>
    ///     Starts a configuration targeting reads of the specified field inside an outer target.
    /// </summary>
    /// <param name="member">The field whose read accesses are matched.</param>
    /// <returns>A configuration whose inner target is a read of <paramref name="member" />.</returns>
    public static PatchConfig InnerGet(FieldInfo member) => new PatchConfig().InnerGet(member);

    /// <summary>
    ///     Starts a configuration targeting writes to the specified property inside an outer target.
    /// </summary>
    /// <param name="member">The property whose setter calls are matched. The property must have a setter.</param>
    /// <returns>A configuration whose inner target is the property's setter.</returns>
    public static PatchConfig InnerSet(PropertyInfo member) => new PatchConfig().InnerSet(member);

    /// <summary>
    ///     Starts a configuration targeting writes to the specified field inside an outer target.
    /// </summary>
    /// <param name="member">The field whose write accesses are matched.</param>
    /// <returns>A configuration whose inner target is a write to <paramref name="member" />.</returns>
    public static PatchConfig InnerSet(FieldInfo member) => new PatchConfig().InnerSet(member);

    /// <summary>
    ///     Starts a configuration targeting occurrences of the specified 32-bit integer constant inside an outer target.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    /// <returns>A configuration whose inner target is <paramref name="value" />.</returns>
    public static PatchConfig InnerConstant(int value) => new PatchConfig().InnerConstant(value);

    /// <summary>
    ///     Starts a configuration targeting occurrences of the specified 64-bit integer constant inside an outer target.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    /// <returns>A configuration whose inner target is <paramref name="value" />.</returns>
    public static PatchConfig InnerConstant(long value) => new PatchConfig().InnerConstant(value);

    /// <summary>
    ///     Starts a configuration targeting occurrences of the specified single-precision floating-point constant inside an outer target.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    /// <returns>A configuration whose inner target is <paramref name="value" />.</returns>
    public static PatchConfig InnerConstant(float value) => new PatchConfig().InnerConstant(value);

    /// <summary>
    ///     Starts a configuration targeting occurrences of the specified double-precision floating-point constant inside an outer target.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    /// <returns>A configuration whose inner target is <paramref name="value" />.</returns>
    public static PatchConfig InnerConstant(double value) => new PatchConfig().InnerConstant(value);

    /// <summary>
    ///     Starts a configuration targeting occurrences of the specified string constant inside an outer target.
    /// </summary>
    /// <param name="value">The constant value to match.</param>
    /// <returns>A configuration whose inner target is <paramref name="value" />.</returns>
    public static PatchConfig InnerConstant(string value) => new PatchConfig().InnerConstant(value);

    /// <summary>
    ///     Starts a configuration implemented by the specified patch method.
    /// </summary>
    /// <param name="method">The static method that implements the patch.</param>
    /// <returns>A configuration whose patch method is <paramref name="method" />.</returns>
    public static PatchConfig With(MethodInfo method) => new PatchConfig().With(method);


    /// <param name="patchConfig">The configuration to copy. Each member leaves this instance unchanged.</param>
    extension(PatchConfig patchConfig)
    {
        /// <summary>
        ///     Gets a copy configured to run before its selected operation.
        /// </summary>
        public PatchConfig Prefix => patchConfig with { Kind = PatchKind.Prefix };

        /// <summary>
        ///     Gets a copy configured to run after its selected operation.
        /// </summary>
        public PatchConfig Postfix => patchConfig with { Kind = PatchKind.Postfix };

        /// <summary>
        ///     Returns a copy targeting the specified outer method or constructor.
        /// </summary>
        /// <param name="method">The outer method or constructor to patch.</param>
        /// <returns>A copy of the configuration whose outer target is <paramref name="method" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="method" /> is neither a <see cref="MethodInfo" /> nor a <see cref="ConstructorInfo" />.
        /// </exception>
        public PatchConfig Of(MethodBase method) => patchConfig with
        {
            Target = method switch
            {
                MethodInfo methodInfo => new MethodInvocation(methodInfo),
                ConstructorInfo constructorInfo => new OuterConstructorInvocation(constructorInfo),
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
            },
        };

        /// <summary>
        ///     Returns a copy targeting calls to the specified method or constructor inside the outer target.
        /// </summary>
        /// <param name="method">The inner method or constructor to match.</param>
        /// <returns>A copy of the configuration whose inner target is <paramref name="method" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="method" /> is neither a <see cref="MethodInfo" /> nor a <see cref="ConstructorInfo" />.
        /// </exception>
        public PatchConfig Inner(MethodBase method) => patchConfig with
        {
            InnerTarget = method switch
            {
                MethodInfo methodInfo => new MethodInvocation(methodInfo),
                ConstructorInfo constructorInfo => new InnerConstructorInvocation(constructorInfo),
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
            },
        };

        /// <summary>
        ///     Returns a copy targeting reads of the specified property inside the outer target.
        /// </summary>
        /// <param name="member">The property whose getter calls are matched. The property must have a getter.</param>
        /// <returns>A copy of the configuration whose inner target is the property's getter.</returns>
        public PatchConfig InnerGet(PropertyInfo member) => patchConfig with
        {
            InnerTarget = new MethodInvocation(member.GetMethod ?? throw new ArgumentException(null, nameof(member))),
        };

        /// <summary>
        ///     Returns a copy targeting reads of the specified field inside the outer target.
        /// </summary>
        /// <param name="member">The field whose read accesses are matched.</param>
        /// <returns>A copy of the configuration whose inner target is a read of <paramref name="member" />.</returns>
        public PatchConfig InnerGet(FieldInfo member) => patchConfig with { InnerTarget = new GetFieldInvocation(member) };

        /// <summary>
        ///     Returns a copy targeting writes to the specified property inside the outer target.
        /// </summary>
        /// <param name="member">The property whose setter calls are matched. The property must have a setter.</param>
        /// <returns>A copy of the configuration whose inner target is the property's setter.</returns>
        public PatchConfig InnerSet(PropertyInfo member) => patchConfig with
        {
            InnerTarget = new MethodInvocation(member.SetMethod ?? throw new ArgumentException(null, nameof(member))),
        };

        /// <summary>
        ///     Returns a copy targeting writes to the specified field inside the outer target.
        /// </summary>
        /// <param name="member">The field whose write accesses are matched.</param>
        /// <returns>A copy of the configuration whose inner target is a write to <paramref name="member" />.</returns>
        public PatchConfig InnerSet(FieldInfo member) => patchConfig with { InnerTarget = new SetFieldInvocation(member) };

        /// <summary>
        ///     Returns a copy targeting occurrences of the specified 32-bit integer constant inside the outer target.
        /// </summary>
        /// <param name="value">The constant value to match.</param>
        /// <returns>A copy of the configuration whose inner target is <paramref name="value" />.</returns>
        public PatchConfig InnerConstant(int value) => patchConfig with { InnerTarget = new ConstantIntInvocation(value) };

        /// <summary>
        ///     Returns a copy targeting occurrences of the specified 64-bit integer constant inside the outer target.
        /// </summary>
        /// <param name="value">The constant value to match.</param>
        /// <returns>A copy of the configuration whose inner target is <paramref name="value" />.</returns>
        public PatchConfig InnerConstant(long value) => patchConfig with { InnerTarget = new ConstantLongInvocation(value) };

        /// <summary>
        ///     Returns a copy targeting occurrences of the specified single-precision floating-point constant inside the outer target.
        /// </summary>
        /// <param name="value">The constant value to match.</param>
        /// <returns>A copy of the configuration whose inner target is <paramref name="value" />.</returns>
        public PatchConfig InnerConstant(float value) => patchConfig with { InnerTarget = new ConstantFloatInvocation(value) };

        /// <summary>
        ///     Returns a copy targeting occurrences of the specified double-precision floating-point constant inside the outer target.
        /// </summary>
        /// <param name="value">The constant value to match.</param>
        /// <returns>A copy of the configuration whose inner target is <paramref name="value" />.</returns>
        public PatchConfig InnerConstant(double value) => patchConfig with { InnerTarget = new ConstantDoubleInvocation(value) };

        /// <summary>
        ///     Returns a copy targeting occurrences of the specified string constant inside the outer target.
        /// </summary>
        /// <param name="value">The constant value to match.</param>
        /// <returns>A copy of the configuration whose inner target is <paramref name="value" />.</returns>
        public PatchConfig InnerConstant(string value) => patchConfig with { InnerTarget = new ConstantStringInvocation(value) };

        /// <summary>
        ///     Returns a copy implemented by the specified patch method.
        /// </summary>
        /// <param name="method">The static method that implements the patch.</param>
        /// <returns>A copy of the configuration whose patch method is <paramref name="method" />.</returns>
        public PatchConfig With(MethodInfo method) => patchConfig with { PatchMethod = method };

        /// <summary>
        ///     Returns a copy with the specified optional behaviors enabled.
        /// </summary>
        /// <param name="options">The <see cref="PatchOptions" /> flags to use, replacing any previously configured flags.</param>
        /// <returns>A copy of the configuration using <paramref name="options" />.</returns>
        public PatchConfig Options(PatchOptions options) => patchConfig with { Options = options };

        /// <summary>
        ///     Returns a copy with the specified execution priority.
        /// </summary>
        /// <param name="priority">The priority value. See <see cref="PatchPriority" /> for standard values.</param>
        /// <returns>A copy of the configuration using <paramref name="priority" />.</returns>
        public PatchConfig Priority(int priority) => patchConfig with { Priority = priority };
    }
}
