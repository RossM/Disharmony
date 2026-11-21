namespace Disharmony;

internal interface IBackend
{
    /// <summary>
    ///     Resolves deferred trampolines until the worklist is empty.
    /// </summary>
    /// <remarks>
    ///     PatchDirectly runs under Harmony's lock. Other threads may add or remove trampolines between iterations.
    ///     Returned failures are reported after releasing the lock, and processing continues. Exceptions from Harmony's
    ///     recovery path or the error handler propagate. This error-handling behavior is provisional.
    /// </remarks>
    void ResolveAllTrampolines();

    void ApplyPatch(MethodBaseInvocation original, Ruleset ruleset, bool useTrampolines, bool debug, bool optimize);
    void Unpatch(MethodBase methodBase);

#if DEBUG
    event Action? ApplyPatchHookForTesting;
#endif
}
