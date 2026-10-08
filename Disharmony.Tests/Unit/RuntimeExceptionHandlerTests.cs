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
        Action<Exception> handler = _ => Assert.That(changePatches, Throws.TypeOf<InvalidOperationException>());
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            Patcher.ReportException(new InvalidOperationException("Patching failed."));
            Assert.That(() => Patcher.Patch(Array.Empty<PatchConfig>()), Throws.Nothing);
            Assert.That(() => Patcher.Unpatch(handle), Throws.Nothing);
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }
    }

    [Test]
    public void Handler_NestedErrorReport_KeepsPatchChangesBlockedUntilOuterHandlerReturns()
    {
        var outer = new InvalidOperationException("Outer failure");
        var inner = new InvalidOperationException("Inner failure");
        Action<Exception> handler = exception =>
        {
            if (exception == outer)
                Patcher.ReportException(inner);
            Assert.That(() => Patcher.Patch(Array.Empty<PatchConfig>()), Throws.TypeOf<InvalidOperationException>());
        };
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            Patcher.ReportException(outer);
            Assert.That(() => Patcher.Patch(Array.Empty<PatchConfig>()), Throws.Nothing);
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }
    }

    [Test]
    public void Handler_Throws_RestoresPermissionToChangePatches()
    {
        var failure = new InvalidOperationException("Handler failed.");
        Action<Exception> handler = _ => throw failure;
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            var reported = Assert.Throws<InvalidOperationException>(() => Patcher.ReportException(new Exception("Patching failed.")));
            Assert.That(reported, Is.SameAs(failure));
            PatchHandle? handle = null;
            Assert.That(() => handle = Patcher.Patch(Array.Empty<PatchConfig>()), Throws.Nothing);
            Assert.That(() => Patcher.Unpatch(handle!), Throws.Nothing);
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }
    }

    [Test]
    public void Handler_DoesNotBlockPatchChangesOnAnotherThread()
    {
        // Report directly so the callback holds neither registration nor Harmony's lock.
        // Waiting for a patching worker from a real patching-error callback is prohibited.
        Action<Exception> handler = _ =>
        {
            Task worker = Task.Run(() =>
            {
                PatchHandle handle = Patcher.Patch(Array.Empty<PatchConfig>());
                Patcher.Unpatch(handle);
            });
            Assert.That(worker.Wait(TimeSpan.FromSeconds(10)), Is.True, "The independent worker did not finish.");
            Assert.That(() => Patcher.Patch(Array.Empty<PatchConfig>()), Throws.TypeOf<InvalidOperationException>());
        };
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        try
        {
            Patcher.ReportException(new InvalidOperationException("Patching failed."));
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }
    }
}
