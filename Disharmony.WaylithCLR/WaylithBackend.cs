using System.Reflection;
using Disharmony.RulesEngine;
using JetBrains.Annotations;
using WaylithClr;

namespace Disharmony.WaylithCLR;

[UsedImplicitly]
internal class WaylithBackend : IBackend
{
    private readonly Transformer transformer;

    public WaylithBackend()
    {
        transformer = new Transformer();
        transformer.Register();
    }


    public void ResolveAllTrampolines() { }

    public void ApplyPatch(MethodBaseInvocation original, Ruleset ruleset, bool useTrampolines, bool debug, bool optimize)
    {
#if DEBUG
        ApplyPatchHookForTesting?.Invoke();
#endif

        // The pipeline passes definition handles to CanTransform. Reflection can return
        // a different handle for a struct override, so registration must use that same identity.
        MethodBase methodBase = original.MethodBase;
        transformer.ApplyPatch(MethodBodyUpdater.GetDefinitionHandle(methodBase), ruleset, useTrampolines, debug, optimize);
    }

    public void Unpatch(MethodBase methodBase)
    {
        transformer.RemovePatch(MethodBodyUpdater.GetDefinitionHandle(methodBase));
    }

#if DEBUG
    public event Action? ApplyPatchHookForTesting;
#endif
}
