using System.IO;
using System.Text;

namespace Disharmony.Tests;

[SetUpFixture]
public sealed class TestRunSetup
{
    private StreamWriter? harmonyLogWriter;

    [OneTimeSetUp]
    public void ConfigureHarmonyLog()
    {
        Assembly harmonyAssembly = typeof(Harmony).Assembly;
        TestContext.Progress.WriteLine($"Harmony: {harmonyAssembly.FullName} ({harmonyAssembly.Location})");
        string? expectedMajor = Environment.GetEnvironmentVariable("DISHARMONY_TEST_HARMONY_MAJOR");
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
