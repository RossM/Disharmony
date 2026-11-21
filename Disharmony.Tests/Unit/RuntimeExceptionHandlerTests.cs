using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Disharmony.Tests.Unit;

[TestFixture]
public sealed class RuntimeExceptionHandlerTests : PatchTestBase
{
    [TestCase("PatchAllAssembly")]
    [TestCase("PatchCategory")]
    [TestCase("PatchAllType")]
    [TestCase("PatchMethods")]
    [TestCase("PatchConfig")]
    [TestCase("PatchConfigAndTargets")]
    [TestCase("TargetAndPatchConfigs")]
    [TestCase("PatchConfigs")]
    [TestCase("Unpatch")]
    public void Handler_PatchChangesAreRejected(string operation)
    {
        MethodInfo target = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.Value))!;
        MethodInfo postfix = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.IncrementResult))!;
        PatchConfig patch = Patch.Postfix.Of(target).With(postfix);
        PatchHandle handle = Patcher.Patch(Array.Empty<PatchConfig>());
        Action changePatches = operation switch
        {
            "PatchAllAssembly" => () => Patcher.PatchAll(typeof(HarmonyInterfaceTargets).Assembly),
            "PatchCategory" => () => Patcher.PatchCategory(typeof(HarmonyInterfaceTargets).Assembly, "test"),
            "PatchAllType" => () => Patcher.PatchAll(typeof(HarmonyInterfaceTargets)),
            "PatchMethods" => () => Patcher.Patch(new[] { postfix }),
            "PatchConfig" => () => Patcher.Patch(patch),
            "PatchConfigAndTargets" => () => Patcher.Patch(patch, new MethodBase[] { target }),
            "TargetAndPatchConfigs" => () => Patcher.Patch(target, new[] { patch }),
            "PatchConfigs" => () => Patcher.Patch(new[] { patch }),
            "Unpatch" => () => Patcher.Unpatch(handle),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        Exception? rejection = null;
        Action<Exception> handler = _ =>
        {
            try
            {
                changePatches();
            }
            catch (Exception exception)
            {
                rejection = exception;
            }
        };
        Patcher.RuntimeExceptionHandler -= RecordRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            Patcher.ReportException(new InvalidOperationException("Patching failed."));
            Assert.That(rejection, Is.TypeOf<InvalidOperationException>());
            Assert.That(() => Patcher.Patch(Array.Empty<PatchConfig>()), Throws.Nothing);
            Assert.That(() => Patcher.Unpatch(handle), Throws.Nothing);
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += RecordRuntimeException;
        }
    }

    [Test]
    public void Handler_NestedErrorReport_KeepsPatchChangesBlockedUntilOuterHandlerReturns()
    {
        var outer = new InvalidOperationException("Outer failure");
        var inner = new InvalidOperationException("Inner failure");
        List<Exception> reports = [];
        List<Exception?> rejections = [];
        Action<Exception> handler = exception =>
        {
            reports.Add(exception);
            if (exception == outer)
                Patcher.ReportException(inner);
            try
            {
                Patcher.Patch(Array.Empty<PatchConfig>());
                rejections.Add(null);
            }
            catch (Exception rejection)
            {
                rejections.Add(rejection);
            }
        };
        Patcher.RuntimeExceptionHandler -= RecordRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            Patcher.ReportException(outer);
            Assert.That(reports, Is.EqualTo(new[] { outer, inner }));
            Assert.That(rejections, Has.Count.EqualTo(2));
            Assert.That(rejections, Has.All.TypeOf<InvalidOperationException>());
            Assert.That(() => Patcher.Patch(Array.Empty<PatchConfig>()), Throws.Nothing);
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += RecordRuntimeException;
        }
    }

    [Test]
    public void Handler_Throws_LogsFailureAndRestoresPermissionToChangePatches()
    {
        var failure = new InvalidOperationException("Handler failed.");
        int handlerCalls = 0;
        Action<Exception> handler = _ =>
        {
            handlerCalls++;
            throw failure;
        };
        using var logStream = new MemoryStream();
        using var logWriter = new StreamWriter(logStream) { AutoFlush = true };
        var previousLogWriter = FileLog.LogWriter;
        FileLog.LogWriter = logWriter;
        Patcher.RuntimeExceptionHandler -= RecordRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            Assert.DoesNotThrow(() => Patcher.ReportException(new Exception("Patching failed.")));
            Assert.That(handlerCalls, Is.EqualTo(1));
            Assert.That(Encoding.UTF8.GetString(logStream.ToArray()), Does.Contain(failure.ToString()));
            PatchHandle? handle = null;
            Assert.That(() => handle = Patcher.Patch(Array.Empty<PatchConfig>()), Throws.Nothing);
            Assert.That(() => Patcher.Unpatch(handle!), Throws.Nothing);
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += RecordRuntimeException;
            FileLog.LogWriter = previousLogWriter;
        }
    }

    [Test]
    public void Handler_DoesNotBlockPatchChangesOnAnotherThread()
    {
        // Report directly so the callback holds neither registration nor Harmony's lock.
        // Waiting for a patching worker from a real patching-error callback is prohibited.
        Task? worker = null;
        bool workerCompleted = false;
        Exception? rejection = null;
        Action<Exception> handler = _ =>
        {
            worker = Task.Run(() =>
            {
                PatchHandle handle = Patcher.Patch(Array.Empty<PatchConfig>());
                Patcher.Unpatch(handle);
            });
            workerCompleted = worker.Wait(TimeSpan.FromSeconds(10));
            try
            {
                Patcher.Patch(Array.Empty<PatchConfig>());
            }
            catch (Exception exception)
            {
                rejection = exception;
            }
        };
        Patcher.RuntimeExceptionHandler -= RecordRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            Patcher.ReportException(new InvalidOperationException("Patching failed."));
            Assert.That(workerCompleted, Is.True, "The independent worker did not finish successfully.");
            Assert.That(worker!.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(rejection, Is.TypeOf<InvalidOperationException>());
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += RecordRuntimeException;
        }
    }
}
