namespace Disharmony.Tests.Support;

public abstract class PatchTestBase
{
    protected static void ThrowRuntimeException(Exception exception) =>
        throw new InvalidOperationException("Runtime exception", exception);

    [SetUp]
    public void UnpatchBeforeTest()
    {
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        HarmonyInterface.Instance.optimizerEnabled = false;
        PatchRegistry.Instance.UnpatchAll();
    }

    [TearDown]
    public void RemoveRuntimeExceptionHandler() =>
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;

    protected static void ApplyPatch(Type patchMethodsType, string patchMethodName) =>
        Patcher.Patch(patchMethodsType.GetMethod(patchMethodName));
}
