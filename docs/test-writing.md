# Writing Disharmony tests

This guide is for contributors who know C# and the patching API. It captures the testing practices learned while building Disharmony's suite: how to make a test readable, what runtime and compiler behavior can hide a bug, and how to respond when a reasonable test fails.

For SDK setup, test runners, runtime support, logs, and hang detection, use [Development and testing](https://github.com/RossM/Disharmony/blob/main/Disharmony.Tests/README.md). For subsystem boundaries, see [Architecture](architecture.md). Paths below are relative to the repository root.

## Start with the contract, not the implementation

Describe what an API user should observe before choosing a fixture. A binding test should establish what value the patch sees or which storage it changes, not merely that the binder selected a particular internal representation.

Use end-to-end and unit tests together when they answer different questions:

- An end-to-end test applies a patch through the public API, calls the target, and checks behavior. It catches binding, code generation, detour, and runtime problems together.
- A unit test supplies controlled inputs to a subsystem and checks its contract directly. It makes an instruction translation or validation failure easy to locate without relying on compiler-generated IL.

A successful binder unit test does not prove the generated patch runs correctly. Conversely, an end-to-end test may pass while an internal transformation silently stops being performed. For example, optimizer fallback can preserve the target's behavior even though optimization failed.

When testing an option or attribute, include evidence that it was recognized. "The method did not crash" does not establish that a class-level setting was inherited, a patch was inlined, or an optimization was applied. Choose an observable distinction or inspect the relevant generated output.

Determine expected behavior from the API contract and, for IL semantics, the relevant [ECMA-335 rules](https://ecma-international.org/publications-and-standards/standards/ecma-335/) (especially Partition III for instructions and stack behavior). Do not calculate expected results using the implementation under test or its lookup tables. Read implementation code to discover paths worth testing, not to define correctness.

Keep each case focused. Split reading from writing rather than making one patch do both. Use visibly different original and replacement values. Assert multiple observations separately; do not add them together or encode them into an arithmetic checksum. When testing skipping, also prove whether the original operation ran: its default return value alone may not distinguish execution from skipping.

## Put tests where their purpose is visible

Use the existing layout:

| Test purpose | Location |
| --- | --- |
| Public patch behavior | `Disharmony.Tests/EndToEnd/<area>` |
| A subsystem's contract | `Disharmony.Tests/Unit/<area>` |
| An individual optimizer pass | `Disharmony.Tests/Unit/Optimizer/Passes` |
| Stable compiled targets and inline patch methods | `Disharmony.TestTargets` |
| Widely reused test infrastructure | `Disharmony.Tests/Support` |
| Compiler diagnostics | `Disharmony.Analyzers.Tests` |

Match namespaces to folders and fixture names to filenames. Organize by the feature or combination of features exercised, not by when tests were written. Extend an appropriate fixture instead of creating a new fixture for every task. Avoid partial classes without a compelling reason.

For an end-to-end case `FooTests.Bar`, use `FooPatches.Bar`. If the test uses several patches, name them `Bar_Prefix`, `Bar_Postfix`, or another meaningful role suffix. Normally these patches belong in `FooTests.cs` beside the fixture. Inline patch methods are the exception: they must live in `Disharmony.TestTargets`, retaining the correspondence with the test names.

Keep target classes in their own files. Reuse simple targets by shape, such as static methods, class instance methods, and struct instance methods, when that does not change the behavior being tested. Preserve separate targets for genuinely different signatures, overloads, compiler-generated shapes, or lifecycle requirements. Optimizer and inline scenarios should have dedicated, logically organized targets rather than acquiring incidental dependencies on unrelated binding targets.

Keep cases self-contained. Introduce a helper only when it will serve at least ten cases; a few repeated setup lines are preferable to hiding the experiment behind a helper. Use the established shared helpers when they already express the required operation clearly.

Keep edits proportionate: adding tests is not permission to change product behavior to satisfy them.

## Make matrices readable and diagnostic

Choose dimensions that can change binding or emitted IL, rather than mechanically multiplying every option. For basic runtime binding coverage, the usual type/access matrix is:

| Bound value | Read by value | Read through `ref` | Write through `ref` |
| --- | --- | --- | --- |
| Primitive | Case | Case | Case |
| Reference type | Case | Case | Case |
| Struct | Case | Case | Case |

Every ref-write case needs a corresponding ref-read case. Reading can expose an invalid address without relying on a write to corrupt memory. Where a combination is unsupported, the case should verify the intended validation rejection instead of quietly omitting it.

Do not confuse the target's parameter passing mode with the patch's parameter passing mode. A target's `ref` argument and a patch's `ref` binding are separate dimensions. Add static/class-instance/struct-instance, inner/outer scope, `in`/`out`, enums, interfaces, nullable types, or conversions when they exercise distinct rules.

New cases for basic bindings can generally use prefixes rather than duplicating the entire matrix for postfixes. Return bindings need postfix coverage too: a prefix cannot observe the original method's non-default result. Retain existing valid prefix and postfix cases.

Separate dimensions with underscores and keep their order predictable within a matrix. For example:

```text
Prefix_LocalFunctionCapturedVariable_Primitive_ReadByValue
Prefix_LocalFunctionCapturedVariable_Primitive_ReadByReference
Prefix_LocalFunctionCapturedVariable_Primitive_WriteByReference
Prefix_LocalFunctionCapturedVariable_ReferenceType_ReadByValue
```

Rename existing cases that become part of an expanded matrix so they follow the same pattern. Keep the full case name as a greppable string in source, including names supplied to `TestCaseData`. Avoid repeating a fixture's subject in every case name: `GetOutputTypeTests` can contain `Add_Int_Int`.

Use individual test methods for behavioral cases whose setup or meaning would be obscured by `[TestCase]`. Data-driven tests are useful for large explicit tables, such as opcode/input-types/expected-type tuples. Keep those rows visible and straightforward; do not build an opaque generator of combinations. Use named arguments for optional case-record parameters, and do not derive the oracle from `OpCodeData`.

To investigate a failure, vary one dimension at a time. Changing both prefix to postfix and integer to string in the next experiment makes the evidence harder to interpret.

## A small end-to-end test

The projects provide global imports for the types used below. This example shows a new fixture and its ordinary, non-inline patch in `Disharmony.Tests/EndToEnd/ParameterBinding/ExampleArgumentBindingTests.cs`:

```csharp
namespace Disharmony.Tests.EndToEnd.ParameterBinding;

[TestFixture]
public sealed class ExampleArgumentBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_Argument_Primitive_WriteByReference()
    {
        Patcher.Patch(typeof(ExampleArgumentBindingPatches).GetMethod(
            nameof(ExampleArgumentBindingPatches.Prefix_Argument_Primitive_WriteByReference))!);

        Assert.That(ExampleArgumentBindingTargets.Identity(7), Is.EqualTo(42));
    }
}

public static class ExampleArgumentBindingPatches
{
    [Prefix]
    [Target(typeof(ExampleArgumentBindingTargets), nameof(ExampleArgumentBindingTargets.Identity))]
    public static void Prefix_Argument_Primitive_WriteByReference(ref int value) => value = 42;
}
```

The target goes in `Disharmony.TestTargets/ExampleArgumentBindingTargets.cs`:

```csharp
namespace Disharmony.Tests;

public static class ExampleArgumentBindingTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Identity(int value) => value;
}
```

The matching parameter name is intentional. The patch does only the write under test, and the assertion observes its effect through the target's return. Prefer reusing an equivalent existing target when adding this kind of case to the real suite.

## Runtime patches need isolation and visible failures

Runtime detours are process-wide. A new object, a new fixture, or a different namespace does not isolate a patched method. Do not opt patching fixtures into parallel execution on the assumption that their instance fields are independent.

Ordinary end-to-end fixtures should derive from `PatchTestBase`. Its per-test setup clears Disharmony patches, disables the optimizer, and subscribes a throwing handler to `Patcher.RuntimeExceptionHandler`; teardown removes that subscription. Reset any additional observation fields or target state used by the fixture. The internal reset API is test infrastructure, not the public patching API being demonstrated.

Use public `Patcher.Patch`/`PatchAll` calls for normal activation, including fluent `PatchConfig` when attributes are not the subject of the test. Internal registration or application calls are appropriate only when they are essential to the behavior under examination. Test selective removal through the returned `PatchHandle` and public unpatch API.

Tests deliberately examining runtime error reporting may replace the default throwing handler, but must restore subscriptions reliably and assert the reported exception. Never allow logging or fallback to make a broken patch appear to pass because the unpatched target still returns the expected value.

Assembly-scanning tests need a separate, controlled assembly, often generated with Reflection.Emit. Do not call `PatchAll` on the main test assembly: it contains unrelated patches, including intentionally invalid ones. Keep category and class-grouping metadata explicit in the generated assembly.

Lifecycle regressions must not rely on test order. Put an apply/unpatch/apply cycle in one test when that is the path being checked. Exercise first-call trampoline resolution separately from later calls and forced application when their behavior matters. Apply cooperating patches in the same call when testing shared state; separate activation calls are a distinct state-isolation case.

For Harmony coexistence, use explicit, valid Harmony patches and a fixture-specific Harmony ID, with cleanup. Invalid Harmony patches and throwing Harmony transpilers can leave corrupted global state; they are not safe tools for provoking a Disharmony validation error.

## Keep compiler-dependent fixtures honest

`Disharmony.TestTargets` is always compiled in Release mode. Put reflection-sensitive target methods and **all inline patch methods** there, even when running the test project in Debug. Mark target methods `NoInlining` where a JIT-inlined call would evade the detour. Do not assume source syntax guarantees an instruction sequence.

For exact IL handling, supply explicit `CodeInstruction` input to the unit API whenever possible. For example, use `InlineRuleBuilder.EmitReplacement` instead of constructing C# whose compilation is supposed to produce `Ldarga_S` or a labeled return. Metadata-only methods in `TestTargets` are fine for providing signatures and local information.

These distinctions have repeatedly mattered:

- **Struct copies versus original storage.** A by-value call can mutate a receiver copy without changing the caller's struct. Observe the copy from within the relevant call when that is the behavior being tested. Passing the struct by reference is a different test, not a fixture repair.
- **Readonly versus writable references.** `in`, `ref`, and `out` have different safety and variance rules. A struct's ordinary `this` is itself passed by reference; do not generalize class-instance or state-machine restrictions to it.
- **Parameters after execution begins.** A compiler may copy an argument to a temporary. A late write to its original storage need not change what the method subsequently uses. Test the documented safe cases and rejection cases separately rather than crafting a fixture that accidentally hides this issue.
- **Ref returns.** Returning the expected value does not establish aliasing. Mutate through the returned reference and check the underlying storage; where useful, change the storage and read through the retained reference. Preserve managed-byref types in expected IL instead of substituting `IntPtr`.
- **Actual captures.** A local function must really use an enclosing variable to create the closure being tested. If it returns a delegate, obtain the expected method from that delegate's `Method`, not a guessed compiler-generated name. Direct patching of a local function and inner patching of its call site exercise different paths.
- **Source methods versus state machines.** Bindings should identify the source-level argument or declaring instance required by the API, not accidentally accept generated `MoveNext`'s receiver. Include static originals, instance originals, and indexes other than zero. Iterator creation and enumeration are separate phases; an outer postfix can run before inner patches execute during enumeration.
- **Virtual dispatch versus IL operands.** A call on a derived receiver may name the base virtual slot in IL. Distinguish the method operand being matched from the override executed at runtime. Use different inner and outer instances when checking a delegate's receiver. Generated delegates can wrap thunks; establish their behavior by invoking them, not by comparing them with ordinary delegates.
- **Constant encodings.** Equal-looking source expressions can compile into different opcodes. Integer literal coverage includes macro values, signed-byte boundaries, and larger encodings. Keep literal values unambiguous in case and target names, separated with underscores.

If Release compilation exposes a failure, inspect the generated IL and reason through what the fixture actually does. Do not switch a by-value path to by-reference, remove a capture, or weaken an assertion just to make it pass.

## Assert IL and graph transformations directly

When checking an exact sequence, show each opcode and operand together in the expected instructions. Use `Support.RuleAssertions.AssertInstructions` or `AssertRules` when applicable. Separate arrays of opcodes and operand assertions make the expected transformation harder to read and easier to misalign.

Include labels, branch destinations, prefixes, and exception markers when they are part of the contract. Write labeled or marked instructions inline with the `WithLabels` and `WithBlocks` params forms. Allocate all input labels from the same `ILGenerator`; unrelated generators' labels are not valid inputs for these tests. `PatchProcessor.CreateILGenerator()` also provides a generator for `LocalBuilder` metadata.

Find newly generated labels through their meaning or the output's label collection instead of fragile positions such as `output[^2]`. Then use those labels in a complete expected sequence. Do not relax an exact transformation test to "contains a store" when the placement of the store is the point.

If a legitimate optimization changes the sequence, validate the new IL before updating the expectation. Keep distinct paths covered: replacing a case's output with a `Dup` sequence should not eliminate the non-`Dup` case.

Pay particular attention to stack discipline across exception regions and exits. [`leave`](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.leave) discards the evaluation stack; values needed afterward must be preserved elsewhere. [`ret`](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ret) does not discard arbitrary stack contents: a void return needs an empty stack, and a value return needs only the returned value. A method or patch that unconditionally throws may have no `ret` at all. Test that replacements do not create dangling labels or unreachable stores that expect nonexistent stack values.

Optimizer tests need both whole-pipeline cases and focused pass cases:

- Whole-pipeline IL tests should assert exact expected output sequences. Include combinations, not just isolated opcode coverage: loops with carried stack values, joins, unreachable blocks, exceptions, numeric conversions, boxed values, structs, and managed references.
- End-to-end optimizer tests must enable `HarmonyInterface.Instance.optimizerEnabled`, retain the throwing runtime-error handler, and restore the flag. Non-optimizer tests leave it disabled. Inline optimization cases must actually request `PatchOptions.Optimize | PatchOptions.Inline`.
- Language coverage should anticipate future passes, not just today's simplifications. Combine data operations with control flow; cover both locals produced by `if` and values carried on the evaluation stack by conditional expressions. Casts, `is`/`as`, null-aware chains, switches, and exception paths can generate materially different IL.
- Inline patches should do only the operation under test. Do not call `RecordPatch` or write extra observation fields to prove execution when testing skipping or a write. Observation fields are appropriate when observing a read is itself the subject of the test.
- Pass tests should check preservation as well as change: arguments, locals, types, identities, regions, labels, edges, and existing operations must survive unless removing them is the pass's job. Check documented postconditions, such as empty edge assignments after merging stack slots.

For type-analysis tests, distinguish declared numeric types from normalized CIL stack types (`int`, `long`, `IntPtr`, and `double`). Test managed references and lattice values explicitly; reflection assignability alone does not define CIL compatibility. Explain special input conventions or supported-but-unverifiable cases in comments beside the table.

Supply valid CIL unless rejection of invalid input is explicitly part of the API contract. Valid but unverifiable CIL is not the same as invalid CIL. Respect documented unsupported features rather than inventing expected graph behavior for them.

## Treat failures as evidence

A test run is a diagnosis, not a contest to make every assertion green. Before changing an unexpected failure:

1. Confirm it against the current checkout. Do not assume an old failure is still present, or exclude it without a baseline run.
2. Trace the intended execution step by step: selected target, binding, actual receiver/storage, patch order, original execution, and final observation.
3. Check that the fixture really contains the intended call, capture, or IL shape, especially under Release compilation.
4. Distinguish an incorrect expectation from a validation hole, a code-generation defect, or an underlying Harmony/runtime limitation.
5. Preserve a reasonable bug-revealing test and report its full name, runtime/configuration, observed behavior, and likely code path. Change the test only with evidence that its expectation or fixture was wrong, or that the supported contract has deliberately changed.

Invalid generated IL is a product bug, including unreachable instructions that a particular runtime happens to tolerate. CLR success is not a substitute for Mono validation. Serious failures that hide subsequent evidence justify pausing broader testing; a small independent failure need not prevent adding other useful cases.

Use `[Ignore]` only for an explicitly accepted unsupported case or external limitation, with a reason. Once validation is intended to reject that case, prefer an expected-exception test. Assert the exception type and relevant structure; avoid exact message assertions unless distinguishing the intended guard from an earlier, unrelated guard is necessary. Validation tests should stop at patch application rather than invoke a method with an invalid patch.

Likewise, a validation test for `PatchOptions.AllowUnsafe` should check that the intended construct is admitted without invoking potentially unsafe generated code. Permission to bypass a safety check is not a promise that arbitrary execution is safe.

Generated IL can crash the host or loop forever; a managed exception assertion cannot protect against either. Run risky cases in a fresh, filtered process with the documented hang protection. Preserve the emitted IL and `disharmony-tests.log` before another run overwrites the log. Avoid introducing a knowingly invalid Harmony patch as a diagnostic shortcut.

## Before handing off a test change

Read the test as someone unfamiliar with the implementation: can they see the scenario, the patch's sole purpose, and the expected behavior without navigating a chain of helpers? Check that its name reveals its matrix dimensions and that equivalent existing cases follow the same pattern.

Run the affected cases first, then relevant broader suites. Follow [Development and testing](https://github.com/RossM/Disharmony/blob/main/Disharmony.Tests/README.md) for Debug/Release and supported CLR, modern .NET, and Mono runs; identify which combinations were actually checked. Set timeouts on inline and other potentially looping tests using the framework-conditional convention, and use process-level hang detection where NUnit cannot abort execution.

Analyzer cases belong in the analyzer project: explicitly enable diagnostics that are disabled by default, and cover both the warning and nearby allowed forms. Analyzer success is not runtime binding coverage.

Keep important fixture assumptions in comments near the tests. Put stable runtime/setup caveats in the testing README and actionable defects or missed optimization opportunities in an issue or appropriate planning record, not in relaxed assertions or a historical list of test counts.
