using System.Threading.Tasks;

namespace Disharmony.Tests.EndToEnd.StateMachines;

public static class AsyncMethodPatches
{
    public static int Executions;
    public static int ParameterObserved;
    public static int FieldObserved;
    public static AsyncMethodTargets? InstanceObserved;

    [Prefix]
    [Target(typeof(AsyncMethodTargets), nameof(AsyncMethodTargets.CallBeforeAndAfterAwait))]
    public static void Prefix_AsyncMethod_ExecutesWhenTaskIsCreated() => Executions++;

    [Postfix]
    [Target(typeof(AsyncMethodTargets), nameof(AsyncMethodTargets.CallBeforeAndAfterAwait))]
    public static void Postfix_AsyncMethod_ExecutesWhenTaskIsCreated() => Executions++;

    [Prefix]
    [Inner(typeof(AsyncInnerMethodTargets), nameof(AsyncInnerMethodTargets.BeforeAwait))]
    [Target(typeof(AsyncMethodTargets), nameof(AsyncMethodTargets.CallBeforeAndAfterAwait))]
    public static void InnerPrefix_AsyncMethod_CallBeforeAwait_ExecutesBeforeSuspension() => Executions++;

    [Postfix]
    [Inner(typeof(AsyncInnerMethodTargets), nameof(AsyncInnerMethodTargets.AfterAwait))]
    [Target(typeof(AsyncMethodTargets), nameof(AsyncMethodTargets.CallBeforeAndAfterAwait))]
    public static void InnerPostfix_AsyncMethod_CallAfterAwait_ExecutesAfterResumption() => Executions++;

    [Postfix]
    [Inner(typeof(AsyncInnerMethodTargets), nameof(AsyncInnerMethodTargets.AfterAwait))]
    [Target(typeof(AsyncMethodTargets), nameof(AsyncMethodTargets.CallBeforeAndAfterAwait))]
    public static void InnerPostfix_AsyncMethod_CallAfterAwait_WritesResult(ref int __result) => __result = 42;

    [Prefix]
    [Inner(typeof(AsyncInnerMethodTargets), nameof(AsyncInnerMethodTargets.AfterAwait))]
    [Target(typeof(AsyncMethodTargets), nameof(AsyncMethodTargets.CallAfterAwait))]
    public static void InnerPrefix_AsyncMethod_OriginalParameter_ReadByValue(int outerValue) =>
        ParameterObserved = outerValue;

    [Prefix]
    [Inner(typeof(AsyncInnerMethodTargets), nameof(AsyncInnerMethodTargets.AfterAwait))]
    [Target(typeof(AsyncMethodTargets), nameof(AsyncMethodTargets.CallAfterAwait))]
    public static void InnerPrefix_AsyncInstanceMethod_DeclaringInstance_ReadByValue(
        [Instance(Scope.Outer)] AsyncMethodTargets instance) => InstanceObserved = instance;

    [Prefix]
    [Inner(typeof(AsyncInnerMethodTargets), nameof(AsyncInnerMethodTargets.AfterAwait))]
    [Target(typeof(AsyncMethodTargets), nameof(AsyncMethodTargets.CallAfterAwait))]
    public static void InnerPrefix_AsyncInstanceMethod_DeclaringField_ReadByValue(
        [Field(nameof(AsyncMethodTargets.Field), Scope.Outer)] int value) => FieldObserved = value;
}

[TestFixture]
#if NETFRAMEWORK
[Timeout(5000)]
#endif
public sealed class AsyncMethodTests : PatchTestBase
{
    [Test]
    public async Task Prefix_AsyncMethod_ExecutesWhenTaskIsCreated()
    {
        AsyncMethodPatches.Executions = 0;
        var gate = new TaskCompletionSource<bool>();
        ApplyPatch(typeof(AsyncMethodPatches), nameof(AsyncMethodPatches.Prefix_AsyncMethod_ExecutesWhenTaskIsCreated));

        Task<int> resultTask = AsyncMethodTargets.CallBeforeAndAfterAwait(gate.Task, 42);

        Assert.That(AsyncMethodPatches.Executions, Is.EqualTo(1));
        Assert.That(resultTask.IsCompleted, Is.False);
        gate.SetResult(true);
        Assert.That(await resultTask, Is.EqualTo(42));
    }

    [Test]
    public async Task Postfix_AsyncMethod_ExecutesWhenTaskIsCreated()
    {
        AsyncMethodPatches.Executions = 0;
        var gate = new TaskCompletionSource<bool>();
        ApplyPatch(typeof(AsyncMethodPatches), nameof(AsyncMethodPatches.Postfix_AsyncMethod_ExecutesWhenTaskIsCreated));

        Task<int> resultTask = AsyncMethodTargets.CallBeforeAndAfterAwait(gate.Task, 42);

        Assert.That(AsyncMethodPatches.Executions, Is.EqualTo(1));
        Assert.That(resultTask.IsCompleted, Is.False);
        gate.SetResult(true);
        Assert.That(await resultTask, Is.EqualTo(42));
    }

    [Test]
    public async Task InnerPrefix_AsyncMethod_CallBeforeAwait_ExecutesBeforeSuspension()
    {
        AsyncMethodPatches.Executions = 0;
        var gate = new TaskCompletionSource<bool>();
        ApplyPatch(
            typeof(AsyncMethodPatches),
            nameof(AsyncMethodPatches.InnerPrefix_AsyncMethod_CallBeforeAwait_ExecutesBeforeSuspension));

        Task<int> resultTask = AsyncMethodTargets.CallBeforeAndAfterAwait(gate.Task, 42);

        Assert.That(AsyncMethodPatches.Executions, Is.EqualTo(1));
        Assert.That(resultTask.IsCompleted, Is.False);
        gate.SetResult(true);
        Assert.That(await resultTask, Is.EqualTo(42));
    }

    [Test]
    public async Task InnerPostfix_AsyncMethod_CallAfterAwait_ExecutesAfterResumption()
    {
        AsyncMethodPatches.Executions = 0;
        var gate = new TaskCompletionSource<bool>();
        ApplyPatch(
            typeof(AsyncMethodPatches),
            nameof(AsyncMethodPatches.InnerPostfix_AsyncMethod_CallAfterAwait_ExecutesAfterResumption));

        Task<int> resultTask = AsyncMethodTargets.CallBeforeAndAfterAwait(gate.Task, 42);

        Assert.That(AsyncMethodPatches.Executions, Is.Zero);
        Assert.That(resultTask.IsCompleted, Is.False);
        gate.SetResult(true);
        Assert.That(await resultTask, Is.EqualTo(42));
        Assert.That(AsyncMethodPatches.Executions, Is.EqualTo(1));
    }

    [Test]
    public async Task InnerPostfix_AsyncMethod_CallAfterAwait_WritesResult()
    {
        var gate = new TaskCompletionSource<bool>();
        ApplyPatch(
            typeof(AsyncMethodPatches),
            nameof(AsyncMethodPatches.InnerPostfix_AsyncMethod_CallAfterAwait_WritesResult));

        Task<int> resultTask = AsyncMethodTargets.CallBeforeAndAfterAwait(gate.Task, 1);
        gate.SetResult(true);

        Assert.That(await resultTask, Is.EqualTo(42));
    }

    [Test]
    public async Task InnerPrefix_AsyncMethod_OriginalParameter_ReadByValue()
    {
        AsyncMethodPatches.ParameterObserved = 0;
        var gate = new TaskCompletionSource<bool>();
        var target = new AsyncMethodTargets { Field = 1 };
        ApplyPatch(
            typeof(AsyncMethodPatches),
            nameof(AsyncMethodPatches.InnerPrefix_AsyncMethod_OriginalParameter_ReadByValue));

        Task<int> resultTask = target.CallAfterAwait(gate.Task, 42);
        Assert.That(AsyncMethodPatches.ParameterObserved, Is.Zero);
        gate.SetResult(true);

        Assert.That(await resultTask, Is.EqualTo(43));
        Assert.That(AsyncMethodPatches.ParameterObserved, Is.EqualTo(42));
    }

    [Test]
    public async Task InnerPrefix_AsyncInstanceMethod_DeclaringInstance_ReadByValue()
    {
        AsyncMethodPatches.InstanceObserved = null;
        var gate = new TaskCompletionSource<bool>();
        var target = new AsyncMethodTargets { Field = 32 };
        ApplyPatch(
            typeof(AsyncMethodPatches),
            nameof(AsyncMethodPatches.InnerPrefix_AsyncInstanceMethod_DeclaringInstance_ReadByValue));

        Task<int> resultTask = target.CallAfterAwait(gate.Task, 10);
        Assert.That(AsyncMethodPatches.InstanceObserved, Is.Null);
        gate.SetResult(true);

        Assert.That(await resultTask, Is.EqualTo(42));
        Assert.That(AsyncMethodPatches.InstanceObserved, Is.SameAs(target));
    }

    [Test]
    public async Task InnerPrefix_AsyncInstanceMethod_DeclaringField_ReadByValue()
    {
        AsyncMethodPatches.FieldObserved = 0;
        var gate = new TaskCompletionSource<bool>();
        var target = new AsyncMethodTargets { Field = 42 };
        ApplyPatch(
            typeof(AsyncMethodPatches),
            nameof(AsyncMethodPatches.InnerPrefix_AsyncInstanceMethod_DeclaringField_ReadByValue));

        Task<int> resultTask = target.CallAfterAwait(gate.Task, 1);
        Assert.That(AsyncMethodPatches.FieldObserved, Is.Zero);
        gate.SetResult(true);

        Assert.That(await resultTask, Is.EqualTo(43));
        Assert.That(AsyncMethodPatches.FieldObserved, Is.EqualTo(42));
    }
}
