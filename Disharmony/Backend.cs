using System.IO;
using System.Runtime.InteropServices;

namespace Disharmony;

internal static class Backend
{
    public static readonly IBackend Instance;
    public static bool optimizerEnabled = false;

    static Backend()
    {
        Instance = Create();
    }

    private static IBackend Create()
    {
        Assembly harmonyAssembly = typeof(Harmony).Assembly;
        string? harmonyInformationalVersion = harmonyAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        string? disharmonyVersion = typeof(Backend).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        bool isWaylithClr = typeof(object).Assembly.GetType("WaylithClr.MethodBodyTransformer") is not null;
        string runtime = isWaylithClr ? "WaylithCLR" : RuntimeInformation.FrameworkDescription;
        Patcher.ReportInformationalMessage($"Running {disharmonyVersion} on {runtime} with {harmonyAssembly.FullName} {harmonyInformationalVersion}.");

        if (!isWaylithClr)
        {
            Patcher.ReportInformationalMessage("Running on Harmony backend: WaylithCLR is unavailable on this runtime.");

            return CreateHarmonyBackend();
        }

        if (!HasActiveMethodBodyInstaller(harmonyAssembly))
        {
            Patcher.ReportInformationalMessage($"Running on Harmony backend: The active Harmony instance does not support WaylithCLR or has WaylithCLR disabled.");
            return CreateHarmonyBackend();
        }

        try
        {
            IBackend waylithBackend = CreateWaylithBackend();

            Patcher.ReportInformationalMessage("Running on WaylithCLR backend.");
            return waylithBackend;
        }
        catch (Exception)
        {
            Patcher.ReportInformationalMessage("Running on Harmony backend: WaylithCLR backend failed to start.");
            return CreateHarmonyBackend();
        }
    }

    private static IBackend CreateWaylithBackend()
    {
        const string waylithBackendAssemblyName = "Disharmony.WaylithCLR";
        Assembly waylithBackendAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                              .FirstOrDefault(assembly => assembly.GetName().Name == waylithBackendAssemblyName)
                                          ?? Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(typeof(Backend).Assembly.Location)!, waylithBackendAssemblyName + ".dll"));
        Type waylithBackendType = waylithBackendAssembly.GetType("Disharmony.WaylithCLR.WaylithBackend", throwOnError: true)!;
        var waylithBackend = (IBackend)Activator.CreateInstance(waylithBackendType, nonPublic: true)!;
        return waylithBackend;
    }

    private static bool HasActiveMethodBodyInstaller(Assembly harmonyAssembly)
    {
        // The bound Harmony copy must participate in the protocol. An older copy can be loaded
        // alongside a registered provider and still install all of its patches as native detours.
        if (harmonyAssembly.GetType("HarmonyLib.MethodBodyPatching") is null)
            return false;

        if (AppDomain.CurrentDomain.GetData("Harmony.MethodBodyPatching.v1") is not object[] { Length: >= 2 } sharedProtocol)
            return false;

        // Harmony synchronizes provider registration on the shared v1 record. Without either
        // the eligibility check (slot 0) or the installer (slot 2), Harmony uses its detours.
        lock (sharedProtocol)
        {
            if (sharedProtocol[0] is object[] { Length: >= 3 } providerParts &&
                providerParts[0] is Func<MethodBase, bool> &&
                providerParts[2] is Action<MethodBase, MethodInfo, Func<Func<object, int>, byte[]>>)
            {
                return true;
            }
        }

        return false;
    }

    private static IBackend CreateHarmonyBackend()
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
