using JetBrains.Annotations;

namespace Disharmony;

internal static class Backend
{
    public static readonly IBackend Instance = Create();

    public static bool optimizerEnabled = false;

    private static IBackend Create()
    {
        Type sharedState = HarmonyInterface.GetHarmonyType("HarmonyLib.HarmonySharedState");

        MethodInfo? FindUpdate(Type payloadType) => sharedState.GetMethod("UpdatePatchInfo",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding, null,
            [typeof(MethodBase), typeof(MethodInfo), payloadType], null);

        if (FindUpdate(typeof(byte[]))?.ReturnType == typeof(void))
            return new HarmonyInterfaceImplV3();
        if (FindUpdate(typeof(HarmonyLib.PatchInfo))?.ReturnType == typeof(void))
            return new HarmonyInterfaceImplV2();

        throw new NotSupportedException($"Unsupported Harmony internals: HarmonyLib.HarmonySharedState.UpdatePatchInfo in {typeof(Harmony).Assembly.FullName}.");
    }
}
