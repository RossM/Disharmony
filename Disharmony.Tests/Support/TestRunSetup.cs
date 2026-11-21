using System.IO;
using System.Text;

namespace Disharmony.Tests;

[SetUpFixture]
public sealed class TestRunSetup
{
    internal static bool IsWaylithClr { get; } =
        typeof(object).Assembly.GetType("WaylithClr.MethodBodyTransformer") is not null;

    private StreamWriter? harmonyLogWriter;

    [OneTimeSetUp]
    public void ConfigureHarmonyLog()
    {
        Assembly harmonyAssembly = typeof(Harmony).Assembly;
        TestContext.Progress.WriteLine($"Harmony: {harmonyAssembly.FullName} ({harmonyAssembly.Location})");
#if WAYLITHCLR_TESTS
        Assembly coreLib = typeof(object).Assembly;
        Assert.That(IsWaylithClr, Is.True, $"Expected WaylithCLR. Loaded CoreLib from {coreLib.Location}.");
        TestContext.Progress.WriteLine($"WaylithCLR: {coreLib.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion} ({coreLib.Location})");
        string adapterPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "HarmonyMod.RuntimeIL.dll");
        Assert.That(File.Exists(adapterPath), Is.True,
            "Set HarmonyRuntimeILDirectory in Directory.Build.local.props to the Harmony mod's Assemblies directory and rebuild the WaylithCLR test project.");
        Assembly.LoadFrom(Path.Combine(TestContext.CurrentContext.TestDirectory, "Waylith.Injection.dll"));
        Assembly adapter = Assembly.LoadFrom(adapterPath);
        adapter.GetType("HarmonyMod.RuntimeILProvider", throwOnError: true)!.GetMethod("Register")!
            .Invoke(null, new object[] { (Action<string>)(message => TestContext.Error.WriteLine(message)) });
        TestContext.Progress.WriteLine($"Harmony runtime adapter: {adapter.Location}");
        const string expectedMajor = "3";
#else
        string? expectedMajor = Environment.GetEnvironmentVariable("DISHARMONY_TEST_HARMONY_MAJOR");
#endif
        if (!string.IsNullOrEmpty(expectedMajor))
            Assert.That(harmonyAssembly.GetName().Version!.Major.ToString(), Is.EqualTo(expectedMajor),
                "The test process did not load the requested Harmony version.");

        string logPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "disharmony-tests.log");
        var logStream = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        harmonyLogWriter = new StreamWriter(logStream, new UTF8Encoding(false)) { AutoFlush = true };

        FileLog.SetBuffer([]);
        FileLog.indentLevel = 0;
        FileLog.LogWriter = harmonyLogWriter;

        TestContext.Progress.WriteLine($"Harmony log: {logPath}");
    }

    [OneTimeTearDown]
    public void CloseHarmonyLog()
    {
        try
        {
            FileLog.FlushBuffer();
        }
        finally
        {
            FileLog.LogWriter = null!;
            harmonyLogWriter?.Dispose();
        }
    }
}
