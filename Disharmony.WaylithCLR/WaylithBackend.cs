using System;
using System.Reflection;
using Disharmony.RulesEngine;
using JetBrains.Annotations;

namespace Disharmony.WaylithCLR;

[UsedImplicitly]
internal class WaylithBackend : IBackend
{
    public void ResolveAllTrampolines() { }

    public void ApplyPatch(MethodBaseInvocation original, Ruleset ruleset, bool useTrampolines, bool debug, bool optimize)
    {
        throw new NotImplementedException();
    }

    public void Unpatch(MethodBase methodBase)
    {
        throw new NotImplementedException();
    }

#if DEBUG
    public event Action? ApplyPatchHookForTesting;
#endif
}
