using System.IO;
using System.Text;
using Disharmony.RulesEngine;

namespace Disharmony.Tests.EndToEnd.Interop;

public static class BackendPatches
{
    public static void DebugOption_LogsModifiedInstructions(ref int __result) => __result = 42;

    public static int HarmonyCalls;
    public static int DisharmonyCalls;

    public static void SharedTarget_BothPostfixesRun_HarmonyPostfix() => HarmonyCalls++;
    public static void SharedTarget_BothPostfixesRun_DisharmonyPostfix() => DisharmonyCalls++;
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
        if (!TestRunSetup.RegistersHarmonyMethodBodyProvider)
            Assert.Ignore("Requires WaylithCLR with Harmony's method-body provider enabled.");

        Type implementation = Backend.Instance.GetType();
        Assert.That(implementation.FullName, Is.EqualTo("Disharmony.WaylithCLR.WaylithBackend"));
        Assert.That(implementation.Assembly.GetName().Name, Is.EqualTo("Disharmony.WaylithCLR"));

        MethodInfo createBackend = typeof(Backend).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)!;
        object backend = createBackend.Invoke(null, null)!;
        Assert.That(backend.GetType(), Is.SameAs(implementation), "The factory must reuse the loaded plugin assembly.");
    }

    [Test]
    public void Instance_LogsHarmony()
    {
        _ = Backend.Instance;
        string startupLog = string.Join("\n", TestRunSetup.BackendStartupMessages);
        Assembly harmonyAssembly = typeof(Harmony).Assembly;
        Assert.That(startupLog, Does.Contain(harmonyAssembly.FullName));
        if (harmonyAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() is { } version)
            Assert.That(startupLog, Does.Contain(version.InformationalVersion));
    }

    [Test]
    public void Instance_OnWaylithClrWithoutProvider_SelectsHarmonyBackend()
    {
        if (!TestRunSetup.IsWaylithClr || TestRunSetup.RegistersHarmonyMethodBodyProvider)
            Assert.Ignore("Requires WaylithCLR without Harmony's method-body provider.");

        Assert.That(Backend.Instance, Is.InstanceOf<HarmonyInterface>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SharedTarget_BothPostfixesRun(bool harmonyFirst)
    {
        const string owner = "Disharmony.Tests.Backend.SharedTarget";
        var harmony = new Harmony(owner);
        MethodInfo target = typeof(HarmonyInterfaceTargets).GetMethod(nameof(HarmonyInterfaceTargets.Value))!;
        var harmonyPostfix = new HarmonyMethod(typeof(BackendPatches).GetMethod(nameof(BackendPatches.SharedTarget_BothPostfixesRun_HarmonyPostfix))!);
        var disharmonyPostfix = Patch.Postfix.With(typeof(BackendPatches).GetMethod(nameof(BackendPatches.SharedTarget_BothPostfixesRun_DisharmonyPostfix))!).Of(target);
        PatchHandle? handle = null;
        BackendPatches.HarmonyCalls = 0;
        BackendPatches.DisharmonyCalls = 0;

        try
        {
            if (harmonyFirst)
                harmony.Patch(target, postfix: harmonyPostfix);
            else
                handle = Patcher.Patch(disharmonyPostfix);

            Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(10));
            Assert.That(BackendPatches.HarmonyCalls, Is.EqualTo(harmonyFirst ? 1 : 0));
            Assert.That(BackendPatches.DisharmonyCalls, Is.EqualTo(harmonyFirst ? 0 : 1));
            BackendPatches.HarmonyCalls = 0;
            BackendPatches.DisharmonyCalls = 0;

            if (harmonyFirst)
                handle = Patcher.Patch(disharmonyPostfix);
            else
                harmony.Patch(target, postfix: harmonyPostfix);

            Assert.That(HarmonyInterfaceTargets.Value(), Is.EqualTo(10));
            Assert.That(BackendPatches.HarmonyCalls, Is.EqualTo(1));
            Assert.That(BackendPatches.DisharmonyCalls, Is.EqualTo(1));
        }
        finally
        {
            if (handle is not null)
                Patcher.Unpatch(handle);
            harmony.Unpatch(target, HarmonyPatchType.All, owner);
        }
    }
}
