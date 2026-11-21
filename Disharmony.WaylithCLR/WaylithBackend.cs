using Disharmony.RulesEngine;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Disharmony.WaylithCLR;

[UsedImplicitly]
internal class WaylithBackend : IBackend
{
    private Transformer transformer;

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

        MethodBase methodBase = original.MethodBase;
        transformer.ApplyPatch(methodBase.MethodHandle, ruleset, useTrampolines, debug, optimize);
    }

    public void Unpatch(MethodBase methodBase)
    {
        transformer.RemovePatch(methodBase.MethodHandle);
    }

#if DEBUG
    public event Action? ApplyPatchHookForTesting;
#endif
}
