using System.IO;

namespace Disharmony;

internal static class Backend
{
    public static readonly IBackend Instance = Create();

    public static bool optimizerEnabled = false;

    private static IBackend Create()
    {
        if (typeof(object).Assembly.GetType("WaylithClr.MethodBodyTransformer") is not null)
        {
            try
            {
                const string assemblyName = "Disharmony.WaylithCLR";
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == assemblyName)
                    ?? Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(typeof(Backend).Assembly.Location)!, assemblyName + ".dll"));
                Type backendType = assembly.GetType("Disharmony.WaylithCLR.WaylithBackend", throwOnError: true)!;
                return (IBackend)Activator.CreateInstance(backendType, nonPublic: true)!;
            }
            catch (Exception)
            {
                // The optional backend may be missing or incompatible with the installed runtime.
                // Continue with Harmony if loading or constructing it fails.
            }
        }

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
