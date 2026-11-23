using System.IO;
using System.Text;
using Disharmony.RulesEngine;

namespace Disharmony.Tests.EndToEnd.Interop;

public static class BackendPatches
{
    public static void DebugOption_LogsModifiedInstructions(ref int __result) => __result = 42;
}

[TestFixture]
public sealed class BackendTests : PatchTestBase
{
    [TestCase(false)]
    [TestCase(true)]
    public void DebugOption_LogsModifiedInstructions(bool debug)
    {
        MethodInfo target = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntResult))!;
        MethodInfo patch = typeof(BackendPatches).GetMethod(nameof(BackendPatches.DebugOption_LogsModifiedInstructions))!;
        FileLog.FlushBuffer();
        using var logStream = new MemoryStream();
        using var logWriter = new StreamWriter(logStream) { AutoFlush = true };
        var previousWriter = FileLog.LogWriter;
        bool previousDebug = Harmony.DEBUG;
        FileLog.LogWriter = logWriter;
        Harmony.DEBUG = false;
        try
        {
            Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
            Patcher.Patch(Patch.Postfix.Of(target).With(patch).Options(debug ? PatchOptions.Debug : PatchOptions.Default));
            Patcher.ForceApply();
            Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(42));
            FileLog.FlushBuffer();

            // The option promises modified IL output, which must include the inserted patch call.
            string log = Encoding.UTF8.GetString(logStream.ToArray());
            Assert.That(log.Contains(nameof(BackendPatches.DebugOption_LogsModifiedInstructions)), Is.EqualTo(debug), log);
        }
        finally
        {
            FileLog.FlushBuffer();
            FileLog.LogWriter = previousWriter;
            Harmony.DEBUG = previousDebug;
        }
    }

    [Test]
    public void PatchAndUnpatch_TakeEffectOnApply()
    {
        MethodInfo first = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        MethodInfo second = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.RegistrationResultB))!;
        var rules = new Ruleset(new Rule
        {
            Mode = OutputMode.InsertBefore,
            Pattern = [new CodeInstruction(OpCodes.Ret)],
            Output = [new CodeInstruction(OpCodes.Ldc_I4, 40), new CodeInstruction(OpCodes.Add)],
        });
        IBackend backend = Backend.Instance;
        bool useTrampolines = backend is HarmonyInterface;

        try
        {
            backend.Patch(new MethodInvocation(first), rules, useTrampolines, debug: false, optimize: false);
            backend.Patch(new MethodInvocation(second), rules, useTrampolines, debug: false, optimize: false);

            Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
            Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(2));

            Assert.That(backend.Apply(), Is.Null);
            Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(41));
            Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(42));

            backend.Unpatch(first);
            backend.Unpatch(second);

            Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(41));
            Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(42));

            Assert.That(backend.Apply(), Is.Null);
            Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
            Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(2));
        }
        finally
        {
            backend.Unpatch(first);
            backend.Unpatch(second);
            Assert.That(backend.Apply(), Is.Null);
        }
    }

    [Test]
    public void Instance_OnWaylithClr_SelectsWaylithBackend()
    {
        if (!TestRunSetup.IsWaylithClr)
            Assert.Ignore("Requires WaylithCLR and the Disharmony.WaylithCLR plugin.");

        Type implementation = Backend.Instance.GetType();
        Assert.That(implementation.FullName, Is.EqualTo("Disharmony.WaylithCLR.WaylithBackend"));
        Assert.That(implementation.Assembly.GetName().Name, Is.EqualTo("Disharmony.WaylithCLR"));

        MethodInfo create = typeof(Backend).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)!;
        object backend = create.Invoke(null, null)!;
        Assert.That(backend.GetType(), Is.SameAs(implementation), "The factory must reuse the loaded plugin assembly.");
    }
}
