using System;
using System.Reflection;
using Disharmony.RulesEngine;
using Disharmony.Utilities;
using JetBrains.Annotations;
using WaylithClr;

namespace Disharmony.WaylithCLR;

[UsedImplicitly]
internal class WaylithBackend : IBackend
{
    private readonly Transformer transformer;
    private readonly UniqueQueue<RuntimeMethodHandle> methodsToRebuild = [];

    public WaylithBackend()
    {
        transformer = new Transformer();
        transformer.Register();
    }


    public void ResolveAllTrampolines() { }

    public void Patch(MethodBaseInvocation original, Ruleset ruleset, bool useTrampolines, bool debug, bool optimize)
    {
#if DEBUG
        ApplyPatchHookForTesting?.Invoke();
#endif

        // The pipeline passes definition handles to CanTransform. Reflection can return
        // a different handle for a struct override, so registration must use that same identity.
        RuntimeMethodHandle method = MethodBodyUpdater.GetDefinitionHandle(original.MethodBase);
        transformer.RegisterPatch(method, ruleset, debug, optimize);
        methodsToRebuild.Enqueue(method);
    }

    public void Unpatch(MethodBase methodBase)
    {
        RuntimeMethodHandle method = MethodBodyUpdater.GetDefinitionHandle(methodBase);
        transformer.RemovePatch(method);
        methodsToRebuild.Enqueue(method);
    }

    public Exception? Apply()
    {
        if (methodsToRebuild.Count == 0)
            return null;

        RuntimeMethodHandle[] methods = [.. methodsToRebuild];
        methodsToRebuild.Clear();

        foreach (MethodBodyTransformFailure failure in transformer.Rebuild(methods))
            Patcher.ReportException(failure.Exception);

        return null;
    }

#if DEBUG
    public event System.Action? ApplyPatchHookForTesting;
#endif
}
