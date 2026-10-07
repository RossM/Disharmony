using JetBrains.Annotations;

namespace Disharmony;

/// <summary>
///     Applies and removes Disharmony patches.
/// </summary>
/// <remarks>
///     <para>
///         A prefix runs before a target method or constructor, and a postfix runs after it. An inner patch runs around
///         matching calls, member accesses, or constants inside the target instead.
///     </para>
///     <para>
///         Patches can be defined in either of two ways:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///                 Attribute-defined patches use <see cref="PrefixAttribute" /> or <see cref="PostfixAttribute" /> with
///                 <see cref="TargetAttribute" /> or <see cref="TargetsAttribute" />. Apply one with
///                 <see cref="Patch(IEnumerable{MethodInfo})" />, all patches declared by a type with
///                 <see cref="PatchAll(Type)" />, or
///                 marked patch classes from an assembly with <see cref="PatchAll(Assembly)" /> or
///                 <see cref="PatchCategory" />.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Programmatic patches use <see cref="Disharmony.Patch" /> to build a <see cref="PatchConfig" />. Choose
///                 <see cref="Disharmony.Patch.Prefix" /> or <see cref="Disharmony.Patch.Postfix" />, add the patch
///                 method with <see cref="Disharmony.Patch.With(MethodInfo)" />, add the target with
///                 <see cref="Disharmony.Patch.Of(MethodBase)" />, then pass the result to
///                 <see cref="Patcher.Patch(PatchConfig)" /> or another programmatic patching overload. Patch-definition
///                 attributes are ignored in this form, but parameter-binding attributes still apply.
///             </description>
///         </item>
///     </list>
///     <para>
///         Patch methods must be static. A prefix can return <see langword="false" /> to skip the target operation; any
///         postfixes still run. Patch parameters bind to target values by name or through the parameter-binding
///         attributes. Passing a bound value by reference can replace it where supported.
///     </para>
///     <para>
///         Every call that applies patches returns a <see cref="PatchHandle" />. Keep the handle if the patches may need
///         to be removed later with <see cref="Unpatch" />. A handle returned for several configurations removes them
///         together, and those configurations can share state as described by <see cref="StateAttribute" />. Patches affect every
///         caller in the current process.
///     </para>
///     <para>
///         Patches take effect before a patching call returns, but generation of the patched method bodies may be
///         deferred until the methods are first called. Use <see cref="ForceApply" /> to complete this work immediately.
///     </para>
/// </remarks>
[PublicAPI]
public static class Patcher
{
    private static PatchRegistry Registry => PatchRegistry.Instance;
    private static HarmonyInterface Harmony => HarmonyInterface.Instance;

    /// <summary>
    ///     Occurs when Disharmony encounters an error while generating or applying patched IL.
    /// </summary>
    /// <remarks>
    ///     Subscribers receive the underlying exception synchronously. When the event has no subscribers, Disharmony
    ///     writes the exception to Harmony's file log. This event does not report exceptions thrown by target or patch
    ///     methods during normal execution.
    /// </remarks>
    public static event Action<Exception>? RuntimeExceptionHandler;

    /// <summary>
    ///     Reports a patching exception to the configured runtime handler or Harmony's file log.
    /// </summary>
    /// <param name="exception">The exception to report.</param>
    internal static void ReportException(Exception exception)
    {
        if (RuntimeExceptionHandler is { } handler)
            handler(exception);
        else
            FileLog.Log($"!!! Unhandled exception: {exception}");
    }

    /// <summary>
    ///     Applies all attributed patches in an assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing the patches.</param>
    /// <remarks>
    ///     Patch classes must be marked with <see cref="PatchAttribute" /> or
    ///     <see cref="HarmonyLib.HarmonyPatch" />. All categories are included; use <see cref="PatchCategory" /> to apply
    ///     only one.
    /// </remarks>
    /// <returns>A handle for removing the patches added by this call.</returns>
    /// <exception cref="PatchException">A patch definition is invalid or a patch could not be applied.</exception>
    public static PatchHandle PatchAll(Assembly assembly)
    {
        PatchHandle handle = new();
        Registry.PatchAll(assembly, handle);
        return handle;
    }

    /// <summary>
    ///     Applies the attributed patches in one category of an assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing the patches.</param>
    /// <param name="category">
    ///     The category to select, or <see langword="null" /> to select classes without a category.
    /// </param>
    /// <remarks>
    ///     Categories are supplied by <see cref="CategoryAttribute" /> or
    ///     <see cref="HarmonyLib.HarmonyPatchCategory" />. Classes must also have a recognized patch marker.
    /// </remarks>
    /// <returns>A handle for removing the patches added by this call.</returns>
    /// <exception cref="PatchException">A patch definition is invalid or a patch could not be applied.</exception>
    public static PatchHandle PatchCategory(Assembly assembly, string? category)
    {
        PatchHandle handle = new();
        Registry.PatchCategory(assembly, category, handle);
        return handle;
    }

    /// <summary>
    ///     Applies all attributed patch methods declared by a type.
    /// </summary>
    /// <param name="type">The type containing the patch methods.</param>
    /// <remarks>
    ///     The type does not need a <see cref="PatchAttribute" /> when patched directly. Inherited methods are not
    ///     processed.
    /// </remarks>
    /// <returns>A handle for removing the patches added by this call.</returns>
    /// <exception cref="PatchException">A patch definition is invalid or a patch could not be applied.</exception>
    public static PatchHandle PatchAll(Type type)
    {
        PatchHandle handle = new();
        Registry.PatchAll(type, handle);
        return handle;
    }

    /// <summary>
    ///     Applies the patches described by the specified attributed methods.
    /// </summary>
    /// <param name="methods">The attributed patch methods to apply.</param>
    /// <remarks>
    ///     Attributes on each method and its declaring type are both considered. The declaring type does not need a
    ///     <see cref="PatchAttribute" /> when methods are patched directly.
    /// </remarks>
    /// <returns>A handle for removing the patches added by this call.</returns>
    /// <exception cref="PatchException">A patch definition is invalid or a patch could not be applied.</exception>
    public static PatchHandle Patch(params IEnumerable<MethodInfo> methods)
    {
        PatchHandle handle = new();
        Registry.Patch(methods, handle);
        return handle;
    }

    /// <summary>
    ///     Applies a configured patch.
    /// </summary>
    /// <param name="patch">The patch to apply.</param>
    /// <returns>A handle for removing the patch.</returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="patch" /> does not specify a patch type, patch method, or outer target.
    /// </exception>
    /// <exception cref="PatchException">The patch definition is invalid or the patch could not be applied.</exception>
    public static PatchHandle Patch(PatchConfig patch)
    {
        return Patch([patch]);
    }

    /// <summary>
    ///     Applies the same configured patch to several methods or constructors.
    /// </summary>
    /// <param name="patch">The patch to apply. The supplied methods replace any target already in the configuration.</param>
    /// <param name="methods">The methods and constructors to patch.</param>
    /// <returns>A handle for removing the patches added by this call.</returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="patch" /> does not specify a patch type or patch method.
    /// </exception>
    /// <exception cref="PatchException">The patch definition is invalid or a patch could not be applied.</exception>
    public static PatchHandle Patch(PatchConfig patch, params IEnumerable<MethodBase> methods)
    {
        return Patch(methods.Select(patch.Of));
    }

    /// <summary>
    ///     Applies several configured patches to the same method or constructor.
    /// </summary>
    /// <param name="method">The method or constructor to patch. It replaces any target in the configurations.</param>
    /// <param name="patches">The patches to apply.</param>
    /// <returns>A handle for removing the patches added by this call.</returns>
    /// <exception cref="ArgumentException">
    ///     A configuration in <paramref name="patches" /> does not specify a patch type or patch method.
    /// </exception>
    /// <exception cref="PatchException">A patch definition is invalid or a patch could not be applied.</exception>
    public static PatchHandle Patch(MethodBase method, params IEnumerable<PatchConfig> patches)
    {
        return Patch(patches.Select(patch => patch.Of(method)));
    }

    /// <summary>
    ///     Applies several configured patches as one group.
    /// </summary>
    /// <param name="patches">
    ///     The patches to apply. Each configuration must specify a patch type, patch method, and target.
    /// </param>
    /// <returns>A handle for removing the group.</returns>
    /// <remarks>
    ///     Patches in the group can share state.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     A configuration in <paramref name="patches" /> does not specify a patch type, patch method, or outer target.
    /// </exception>
    /// <exception cref="PatchException">A patch definition is invalid or a patch could not be applied.</exception>
    public static PatchHandle Patch(params IEnumerable<PatchConfig> patches)
    {
        PatchHandle handle = new();
        Registry.Patch(patches, handle);
        return handle;
    }

    /// <summary>
    ///     Removes patches added by an earlier patching call.
    /// </summary>
    /// <param name="handle">The handle returned by that call.</param>
    /// <remarks>
    ///     Patches added by other calls remain active, even when they patch the same methods.
    /// </remarks>
    /// <exception cref="RuntimePatchException">One or more affected methods could not be updated.</exception>
    public static void Unpatch(PatchHandle handle)
    {
        Registry.Unpatch(handle);
    }

    /// <summary>
    ///     Applies all deferred patches immediately, generating the patched method bodies before their first call.
    /// </summary>
    /// <remarks>
    ///     Patches are already active without this call. Use it during a convenient idle period when avoiding first-use
    ///     delay matters. Patches added afterward may require another call.
    /// </remarks>
    /// <exception cref="RuntimePatchException">One or more deferred patches could not be applied.</exception>
    public static void ForceApply()
    {
        Harmony.ResolveAllTrampolines();
    }
}
