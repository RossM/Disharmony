# Disharmony test coverage plan

This file records deferred test-coverage proposals. They are ideas to evaluate, not agreed requirements or descriptions
of currently supported behavior. Before implementing a section, re-read the implementation and existing tests, confirm
the intended public contract, and refine the cases below rather than mechanically treating this as a checklist.

## Cross-runtime execution

The same end-to-end suite should run regularly on both the Microsoft CLR and Mono. Add both modes to the normal
validation process or CI; an available local runner alone does not protect against regressions.

Suggested coverage:

- Run all non-ignored end-to-end tests under both runtimes, using the same Release-built `TestTargets` assembly.
- Preserve identical assertions and ignore lists across runtimes. A runtime-specific failure should be investigated as
  a product bug before considering a runtime-specific expectation.
- Give particular attention to generated IL containing exception regions, unreachable blocks, by-ref values, value-type
  instances, constructors, state machines, inlining, and control flow that carries stack values.
- Run each runtime in a fresh process so cached trampolines, resolved methods, and static state cannot hide first-use
  failures.

Completion criterion: both runtime commands are repeatable and are run automatically or are an explicit required local
check, with result files retained separately.

## Harmony coexistence failure isolation

Possible future coverage:

- Verify an application failure or rollback on one side does not remove or corrupt the other side's registrations.

Avoid deliberately invalid Harmony patches and throwing Harmony transpilers in this fixture. Harmony may emit invalid
IL or retain a broken patch state instead of rolling back, so those cases cannot safely provide isolated coexistence
tests.

The purpose is to protect ownership and cache invalidation at the boundary between the two systems, not to duplicate
Harmony's own tests.

## Concurrent patch lifecycle and first-use resolution

`ForceApply` now delegates directly to `HarmonyInterface.ResolveAllTrampolines` and does not access patch-registry state.
Its concurrency tests should therefore exercise `HarmonyInterface` directly. Its public methods serialize access to the
trampoline and method-patch collections with Harmony's internal locker, but the precise behavior of operations that
overlap still needs testing and documentation. Separately decide which concurrent registry operations are supported;
unsupported combinations should fail deterministically and early rather than corrupting state or generating invalid IL.

Candidate scenarios:

- Race `ResolveAllTrampolines` with first-call resolution of the same trampoline and verify that it is resolved exactly
  once.
- Resolve different newly patched methods simultaneously.
- Race `ResolveAllTrampolines` with applying and unpatching the same and different methods.
- Verify and document the non-snapshot behavior when a trampoline is added between iterations of
  `ResolveAllTrampolines`; ensure continuous additions cannot create unacceptable starvation.
- Multiple patches for one target are submitted concurrently, where supported, and the final nesting order remains
  deterministic.
- Patch and selectively unpatch different targets concurrently.
- A failure during deferred trampoline resolution races with another caller; the runtime exception is reported once as
  intended, every caller receives contract-compliant behavior, and the original method is restored.
- A `RuntimeExceptionHandler` invokes unrelated patched code, to exercise safe reentrancy without making the test depend
  on recursive patch application unless that is explicitly supported.

Use barriers rather than timing sleeps, repeat race-sensitive cases enough to be meaningful, and apply timeouts so a
deadlock produces a useful test failure.

## Public lifecycle stress and atomicity

Future stress sequences include:

- Inject application failures at different positions after pre-existing patches have reached a mixture of pending and
  resolved states; verify the call remains atomic and rollback restores every prior registration.
- Repeatedly cycle successful and failed apply-unpatch-apply operations across several targets to catch stale registry,
  matcher, trampoline, and thunk cache entries.
- Resolve several pending trampolines when one resolution fails, then verify that later resolution or unpatch operations
  handle every remaining trampoline correctly.
- Throw partway through lazy `MethodInfo` and `PatchConfig` enumerables and verify already-processed entries are rolled
  back without leaving pending work.

Assertions should cover externally observable method behavior and exception reporting, not private collection counts.

## Compiler-generated method families

Other compiler-generated forms can take distinct reflection and state-machine paths. The most important candidate is
async code. Before writing broad matrices, establish whether each form is intended to be supported; if it is not, add
patch-time validation tests for the documented rejection.

Forms worth evaluating:

- Async instance and static methods, both before and after the first invocation.
- Calls inside an async state machine before and after an `await` suspension point.
- Async lambdas and local functions, including captured parameters and instance state.
- Async iterators, which combine iterator and async state-machine transformations.
- Generic local functions and lambdas on generic declaring types.
- Explicit interface implementations and default interface methods, particularly virtual/base-method and `[Method]`
  delegate binding.

For supported state machines, write tests from the API user's perspective: patch the source-level method/call described
by the public attributes or `PatchConfig`, then assert source-level behavior. Do not assert compiler-generated names or
layout except in focused reflection unit tests.

## Additional boundary targets

Possible future coverage:

- Function-pointer signatures, if they can be represented safely by the target framework and C# compiler used by the
  Release-built `TestTargets` project.
- Additional closed-generic shapes, such as instance methods and nested generic declaring types, if generic trampoline
  handling changes.
- Revisit dependency-limited method shapes when the relevant Harmony, MonoMod, or runtime behavior changes.

## Behavioral preservation corpus

Maintain a corpus of Release-compiled target methods that combine language and IL features rather than testing every
feature only in isolation. Apply a no-op outer patch, no-op inner patch, and—where meaningful—inline/optimized variants,
then compare behavior with an unpatched equivalent over representative inputs.

Combinations should cross exception handling, branching, loops, switches, pattern matching, casts, null-aware access,
value types, references, by-ref parameters, generics, closures, and state machines. Include both normal and exceptional
outcomes. This provides broad regression protection when rule generation, exception fixup, trampoline generation, or
the optimizer changes without tying expectations to a particular generated opcode sequence.

## Test-writing constraints

- Keep tests organized by the feature whose public behavior they primarily exercise.
- Put reflection-sensitive target and inline patch methods in `Disharmony.TestTargets`, which is compiled in
  Release mode.
- End-to-end fixtures must fail immediately when `Patcher.RuntimeExceptionHandler` reports an unexpected exception.
- Enable the optimizer only in optimizer fixtures; keep it disabled everywhere else.
- Preserve valid tests that expose possible product bugs. Report and investigate the failure instead of weakening the
  expectation.
- Keep distinct by-value and by-reference struct paths separate; compiler optimizations can make them fundamentally
  different.
- Prefer self-contained tests. Introduce a helper only when at least ten tests genuinely benefit from it.
- Preserve CRLF line endings.
