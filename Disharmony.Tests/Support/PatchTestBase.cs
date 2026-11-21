using System.Collections.Concurrent;

namespace Disharmony.Tests.Support;

public abstract class PatchTestBase
{
    private ConcurrentQueue<Exception> runtimeExceptions = new();

    protected void RecordRuntimeException(Exception exception) => runtimeExceptions.Enqueue(exception);

    [SetUp]
    public void UnpatchBeforeTest()
    {
        runtimeExceptions = new();
        Patcher.RuntimeExceptionHandler -= RecordRuntimeException;
        Patcher.RuntimeExceptionHandler += RecordRuntimeException;
        Backend.optimizerEnabled = false;
        PatchRegistry.Instance.UnpatchAll();
    }

    [TearDown]
    public void CheckRuntimeExceptions()
    {
        Patcher.RuntimeExceptionHandler -= RecordRuntimeException;
        Assert.That(runtimeExceptions, Is.Empty, "Unexpected errors were reported while applying or removing patches.");
    }

    protected static void ApplyPatch(Type patchMethodsType, string patchMethodName) =>
        Patcher.Patch(patchMethodsType.GetMethod(patchMethodName)!);
}
