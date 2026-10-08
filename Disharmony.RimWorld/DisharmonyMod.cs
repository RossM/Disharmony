using System.Threading.Tasks;
using Verse;

namespace Disharmony.RimWorld;

/// <summary>
///     RimWorld's entry point for the Disharmony library mod.
/// </summary>
public sealed class DisharmonyMod : Mod
{
    /// <summary>
    ///     Initializes the Disharmony mod loaded by RimWorld.
    /// </summary>
    /// <param name="content">The mod's content pack.</param>
    public DisharmonyMod(ModContentPack content) : base(content)
    {
        LongEventHandler.QueueLongEvent(EagerApplyPatches, null, true, e => { Log.Error($"Disharmony patching error: {e}"); });
    }

    private static void EagerApplyPatches()
    {
        Task.Factory.StartNew(Patcher.ForceApply, TaskCreationOptions.LongRunning);
    }
}
