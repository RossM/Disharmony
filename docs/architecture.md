# Disharmony architecture

This guide explains the architectural ideas needed to read Disharmony's implementation. It assumes familiarity with C# and Disharmony's public API. For build commands and runtime validation, see [Development and testing](obj/guides/testing.md).

Start with the component map and the two stages below, then follow each stage in more detail. The [glossary](#glossary) provides a quick reference for the terminology used throughout.

## Architecture at a glance

Disharmony implements both whole-method and inner patches by rewriting the outer method's IL, the compiled .NET instruction sequence. It first describes the edits to make, then applies those descriptions to the method body.

Harmony supplies the machinery that makes the rewritten body executable. It builds a replacement method, called a **wrapper**, and installs a **detour** that redirects calls to it. Disharmony participates in that process through a **transpiler**: a callback that receives instructions from Harmony and returns the rewritten sequence.

A Disharmony prefix or postfix becomes code within that sequence. For each outer method, Harmony sees Disharmony's transpiler as one registered patch, even when the sequence incorporates many Disharmony patches.

### Five main components

The runtime library divides the work among five component groups. The registry, binder, and builders prepare the changes. The rule engine applies them, while HarmonyInterface coordinates installation and deferred generation with Harmony.

A **rule** describes an IL edit: where it applies and which instructions to emit. A **ruleset** collects the edits for one outer method. These are the descriptions passed from preparation to method generation.

| Component | Role in the pipeline |
| --- | --- |
| [Patcher and PatchRegistry](#registration-prepare-the-changes) | Patcher receives registration and removal requests. PatchRegistry resolves definitions, coordinates validation, and retains the active patches grouped by outer method. |
| [ParameterBinder](#bindings-describe-value-sources) | Resolve each patch parameter to a value source and check its type and reference access. The result is binding metadata for the rule builders. |
| [RulesetGenerator and rule builders](#rulesets-describe-the-il-edits) | Turn the validated patches for an outer method into one ruleset, including instructions for obtaining the bound values. |
| [RulesEngine](#apply-the-rules) | Match rules against the instruction sequence Harmony supplies, then emit the rewritten body with its locals and branch labels resolved. |
| [HarmonyInterface and adapters](#method-generation-create-the-replacement) | Connect the ruleset to Harmony's wrapper generation, manage deferred generation, and handle version-specific metadata and detour APIs. |

### Two stages of patching

These components run at different times. **Registration** resolves the patch definitions and constructs rulesets. **Method generation** applies those rules to actual instructions and asks Harmony to build the executable replacements.

Disharmony defers method generation by installing a **trampoline**: a small temporary method that receives calls to the target. On its first call, it generates the replacement and forwards the waiting call to it. That first call executes the patched behavior; subsequent calls go directly to the replacement.

| Stage | Starts with | Produces |
| --- | --- | --- |
| Registration | Patch definitions from attributes or fluent registration | Validated patch descriptions, a ruleset for each affected method, and a trampoline |
| Method generation | A pending target and its ruleset | Rewritten IL and an installed Harmony wrapper |

This division is important when reading the validation code. Registration can check a selected operation's signature before inspecting the outer method's body. Whether that operation actually occurs in the body is established later, when the rule engine looks for matches.

## Registration: prepare the changes

During registration, Disharmony works with reflection metadata and types. It needs descriptions of the operations being patched and of the values to pass to patch parameters. Those descriptions give the builders enough information to prepare IL edits without rewriting the outer method yet.

[Patcher.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/Patcher.cs) passes requests to [PatchRegistry.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/PatchRegistry.cs). Attribute discovery constructs the same `PatchConfig` objects accepted by fluent registration, so both reach `ProcessPatch`. [ReflectionTools.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/ReflectionTools.cs) handles member lookup; subsequent processing works with resolved members rather than their names.

### Invocation: a common model for operations

A method call, a field read, and a constant load have different reflection metadata and IL opcodes. The binder and builders need a common way to reason about all of them. [Invocation.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/Invocation.cs) supplies that abstraction: an operation has inputs, a result type, and instructions that perform it.

These inputs and results correspond to values on the IL **evaluation stack**. Loads push values onto it; a call consumes its inputs and pushes its result, if any. Local variables provide storage separate from this stack.

| Operation | Invocation subtype | Inputs | Result | Instruction |
| --- | --- | --- | --- | --- |
| Static `GetPrice(int itemId)` | `MethodInvocation` | An `int` | An `int` | `call` |
| Read an instance field | `GetFieldInvocation` | The object containing the field | The field's value | `ldfld` |
| Write an instance field | `SetFieldInvocation` | The object and the new value | None | `stfld` |
| Load an integer constant | `ConstantIntInvocation` | None | An `int` | `ldc.i4` |

For an instance method, the input list includes the instance before the formal arguments. The machinery can therefore save, load, and bind the instance as another input. The same abstraction describes patch methods as well as targets.

Constructors illustrate why the abstraction describes an operation, rather than just wrapping its reflection member. `InnerConstructorInvocation` represents `newobj`: it consumes constructor arguments and produces an object. `OuterConstructorInvocation` describes execution inside the constructor: it has an instance input and returns `void`. These are different operations despite referring to the same `ConstructorInfo`.

Alongside this description of inputs and results, invocations provide identity. Two descriptions of the same operation compare equal, allowing the generator to group patches around that operation. `EmptyInvocation` represents the absence of an inner target, which distinguishes whole-method patches in the builders.

The registry must also locate the body that contains the selected operation. An iterator or async method's declared body starts a compiler-generated state machine; much of its execution takes place in that state machine's `MoveNext` method. For inner patches, the registry redirects to `MoveNext`. [ReflectionExtensions.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/ReflectionExtensions.cs) locates that implementation. The binder receives both methods so it can resolve original arguments and instance references through state-machine fields where supported.

### Bindings describe value sources

Once the operations are resolved, Disharmony can determine how to supply each patch parameter. [ParameterBinder.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/ParameterBinder.cs) creates a `ParameterBinding` describing the value source, its scope, and the access required by the parameter. It also checks type and reference compatibility.

A binding is metadata, not a value or an instruction sequence. It might identify an argument index, a field chain, the operation's result, or shared patch state. Later, [RuleBuilder.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RuleBuilders/RuleBuilder.cs) turns that description into instructions that obtain the value.

The outer method's arguments already have argument slots that generated code can load. An inner call's inputs, however, are values on the evaluation stack, not additional arguments of the outer method. The inner builder saves them to locals so patches can access them. The binding tells the builder which saved input to use.

Reference access changes the emitted code as well. To pass a result to a `ref` parameter, the builder must store the result and load its address, rather than pass a copy of the value. The inner-postfix example below shows those instructions.

`ProcessPatch` collects the bindings, patch and inner invocations, and execution options into `PatchInfo`, the validated internal patch description. The registry retains these descriptions grouped by outer method, with registration ownership recorded. Changes to the registered patches cause the combined ruleset for an affected method to be rebuilt.

### Rulesets describe the IL edits

The registry now has enough information to describe how each outer method should change. [RulesetGenerator.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RulesetGenerator.cs) creates a whole-method builder and an inner builder for each group of equivalent inner invocations. Their output becomes that method's ruleset.

A rule can locate instructions through an **instruction pattern** and supply an **output template** to replace them or insert code beside them. Other rules emit code at method entry or at the end of the body without matching a pattern.

Building a template does not allocate storage in the eventual replacement method. That method may already need locals from the original body or other patches, so the builders cannot choose its final local indexes in advance.

Instead, they use **placeholders** for locals and branch destinations. The builders share a `RuleBuilderContext` with a temporary `ILGenerator`, the .NET object used to allocate locals and labels when constructing a method. Local placeholders carry their types; label placeholders identify branch destinations. During rule application, the engine maps them to locals and labels belonging to the actual replacement.

The builders use this model in three related ways: preserve the stack around inner operations, join whole-method control flow, and provide shared state storage.

#### Preserve an inner operation's inputs and result

An inner patch must fit into the surrounding instructions with the same stack effect as the original operation: consume the expected inputs and leave the expected result.

Consider a postfix `AdjustPrice(ref int __result)` around a call to the hypothetical static method `GetPrice(int itemId)`. The original `call GetPrice` instruction consumes `itemId` and leaves the price on the stack. [InfixRuleBuilder.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RuleBuilders/InfixRuleBuilder.cs) replaces it with code that saves the input, runs the call and postfix, then leaves the adjusted price:

```text
stloc itemId                  // Save the call's input.
ldloc itemId                  // Restore it for the original call.
call GetPrice
stloc result                  // Save the call's output.
ldloca result                 // Supply its address to the postfix.
call AdjustPrice
ldloc result                  // Leave the adjusted price on the stack.
```

Here, `itemId` and `result` name locals introduced by the template; their initialization instructions are omitted. Saving inputs gives the patches stable locations from which to read or modify them. With several inputs, the builder stores them in reverse order because the last input is at the top of the stack.

A field access or constant load fits the same model. Its `Invocation` supplies the inputs, result type, and operation instructions. The builder can surround it using the same machinery.

#### Join whole-method control flow

A whole-method prefix can be inserted at the body's entry point. Postfixes require more control-flow changes: a method can return from several places, and every normal return must reach the postfix code. Simply appending postfix calls would leave earlier returns untouched.

[CircumfixRuleBuilder.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RuleBuilders/CircumfixRuleBuilder.cs) emits rules that redirect returns to a common exit containing the postfix code. Where needed, each return stores its value in a local before branching there.

Both the whole-method and inner builders use [PrefixPostfixRuleBuilder.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RuleBuilders/PrefixPostfixRuleBuilder.cs) to emit parameter loads and patch calls. An ordinary prefix's Boolean result becomes a conditional branch that skips the operation. The shared emitter also constructs the exception handling described under [exception regions](#keep-exception-regions-valid).

#### Allocate shared state

The locals used to preserve one operation's inputs serve that operation's patches. Shared patch state has an additional requirement: bindings in different patch methods must refer to the same variable, while unrelated registrations must remain separate.

The binder identifies this storage with a state key containing the state name and registration identity; assembly discovery contributes the patch class name as well. Matching keys tell the generator which bindings must share a local.

[StateBuilder.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RuleBuilders/StateBuilder.cs) maps those keys to typed local placeholders and emits initialization at method entry. The resulting storage belongs to each execution of the method. The registry retains descriptions of the state bindings, not the state values themselves.

### Install a trampoline

The ruleset now describes the patches, but no executable replacement body has been built. Registration must make the patches active before returning. It does this by redirecting calls to the temporary trampoline introduced in the overview.

[HarmonyInterface.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/HarmonyInterface.cs) retains the ruleset and generation options in `methodPatches`. Its shared transpiler uses this map to find the edits for a particular method.

Harmony also keeps **patch metadata** recording registered patches and their owners. HarmonyInterface's adapter adds the shared transpiler to that metadata once per outer method, under Disharmony's owner ID. This entry tells Harmony to call the transpiler when generating the wrapper; the transpiler then retrieves the method's combined ruleset.

`MakeTrampoline` constructs a dynamic method with the target's inputs and return type. `EmitTrampoline` writes forwarding code that will call a **resolver**, the Disharmony callback that requests generation for this target. It does not copy the target body or apply the ruleset.

The adapter installs a detour from the target to this trampoline and publishes the patch metadata with the installed replacement. The `trampolines` map records that the target is awaiting generation. Registration is now complete: a call to the target will enter the trampoline rather than execute the unchanged body.

## Method generation: create the replacement

Now follow a caller invoking that registered target. The trampoline is already installed, and the ruleset is waiting in HarmonyInterface. Before the call can execute the target body, Disharmony must ask Harmony to build and install its wrapper.

### Enter through a trampoline

The forwarding code constructed during registration runs in this order:

1. The trampoline loads the caller's arguments onto the evaluation stack and calls the resolver. It passes the target's `MethodBase` so the resolver knows which method to generate.
2. The resolver requests wrapper generation from Harmony. Harmony calls Disharmony's transpiler, which applies the waiting ruleset as described in the next section.
3. The completed wrapper is installed and the target's detour is updated before the resolver returns.
4. The trampoline calls the original target with the waiting arguments. That call now reaches the wrapper because the detour has changed.

The final call uses IL's tail-call instruction, allowing it to discard the trampoline's own call frame. Subsequent callers reach the wrapper directly; the trampoline is no longer their destination.

`ResolveTrampolineImpl` is the resolver path used by this first call. `ForceApply` bypasses the forwarding code: `ResolveAllTrampolines` walks the pending targets and requests wrapper generation without invoking those targets. Both paths use the same stored rulesets.

### Apply the rules

Harmony generates a wrapper from the target body and its registered patches. During that work, it calls Disharmony's transpiler with the current instruction sequence and the wrapper's IL generator. The transpiler retrieves the stored ruleset and passes it, the instructions, and the generator to the [rules processor](https://github.com/RossM/Disharmony/blob/main/Disharmony/RulesEngine/Processor.cs).

The builders have already described the edits. The processor must now find where those descriptions apply and resolve their placeholders against this method.

The processor finds matches before emitting the rewritten sequence. It checks required match counts and rejects overlapping replacements. It recognizes equivalent compact and expanded IL forms, so a pattern need not depend on the exact opcode spelling in the input.

Placeholder mapping connects the templates to real storage and branch destinations. For example, if a pattern matches a load from an existing local, output references to that pattern's local must reuse the same local. The same applies to labels found by a pattern.

New output placeholders instead require new locals or labels. Their mappings belong to each match unless marked as shared across rules. The whole-method return rules, for example, must all branch to the same postfix exit label. [Ruleset.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RulesEngine/Ruleset.cs) defines these mappings.

Rules run in **phases**. All rules in one phase match the same input; their combined output becomes the next phase's input. This allows one transformation to work on code inserted by another. `Ruleset.MatchAndReplace` replaces the input list only after all phases succeed.

Inlining uses that ordering. The first phase inserts patch calls; later rules from [InlineRuleBuilder.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RuleBuilders/InlineRuleBuilder.cs) replace those calls with patch bodies. The builder remaps arguments, locals, branches, and returns so the copied body fits within the outer method. Bodies with exception blocks remain as calls.

### Keep exception regions valid

Correct matches do not by themselves guarantee valid IL. Exception handling places constraints on the evaluation stack that ordinary instruction sequences do not have. A patch that introduces a try/catch region therefore needs to account for the surrounding body's stack values as well.

Disharmony introduces such regions for `AlwaysRun` postfixes. The shared prefix/postfix emitter encloses ordinary prefixes, the operation, and ordinary postfixes in a try/catch. The catch saves the exception and captures its dispatch information to preserve the original throw location. The `AlwaysRun` postfixes execute outside the region, followed by rethrowing if an exception remains. [RuntimeHelpers.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/RuntimeHelpers.cs) supplies that generated-code operation.

The inner operation's inputs do not account for everything already on the stack. For example, in `subtotal + GetPrice(itemId)`, the stack can hold `subtotal` beneath the call's inputs. Saving `itemId` for the patches leaves that other value on the stack, but it cannot remain live across the new exception region's boundary.

After rule application, [ExceptionFixup.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony/ExceptionFixup.cs) saves such values to locals and restores them around exception boundaries. This separate pass accounts for the surrounding instructions, which the inner operation's input and result descriptions alone cannot describe.

### Install the Harmony wrapper

After the transpiler returns the rewritten instructions, Harmony finishes generating and installing the wrapper. It must retain the other Harmony patches on the target as well as the changes made by Disharmony.

Harmony uses the patch metadata retained during registration to combine patches from all owners. Disharmony's entry supplies the shared transpiler, whose rewritten body already includes the Disharmony prefixes and postfixes. Other owners' patches remain part of the same generation process.

The [Harmony 2 adapter](https://github.com/RossM/Disharmony/blob/main/Disharmony/HarmonyInterfaceImplV2.cs) and [Harmony 3 adapter](https://github.com/RossM/Disharmony/blob/main/Disharmony/HarmonyInterfaceImplV3.cs) encapsulate the version-specific APIs for wrapper generation, metadata publication, and detours. `HarmonyInterface.Create` selects an adapter by probing signatures in the Harmony assembly Disharmony is bound to.

These adapters coordinate replacement installation with metadata publication. Their registration path installs the trampoline directly; their generation path asks Harmony to build the wrapper. The two paths must leave Harmony with metadata describing the installed replacement, even though they use different internal APIs to get there.

## Constraints across components

Registration and first-call generation can run on different threads. They share the rulesets and pending-target information described above, and both interact with Harmony's own patch state. Reading these paths requires understanding which lock protects each part of that state and what happens when a callback crosses the boundary.

### Lock ownership and callbacks

The registry's lock protects patch definitions and changes to their combined rulesets. Harmony's lock protects wrapper generation and the adapter's ruleset and trampoline maps. **When both are needed, acquire the registry lock before Harmony's lock.** Harmony calls the transpiler with its lock already held, so calling back into registration from that path would invert the order.

The same Harmony lock coordinates concurrent first calls. `ResolveTrampolineImpl` removes the pending entry before generating the wrapper, while holding the lock. A second resolver waits, finds the entry already removed, and returns. Its trampoline can then forward to the installed replacement.

Error callbacks can also run under patching locks. [Patcher.ReportException](https://github.com/RossM/Disharmony/blob/main/Disharmony/Patcher.cs) invokes handlers synchronously and surrounds their execution with a thread-local flag. Registration and removal reject calls while that flag is set, preventing a handler from re-entering registration on the reporting thread.

The flag cannot prevent a handler from waiting for another thread that needs a held lock. Reading the reporting code therefore requires considering both the callback and the locks held by its caller.

### Generation failures and execution exceptions

A failure to construct a wrapper needs different handling from an exception thrown while the wrapper executes. The library's error handler reports problems running the preparation and generation pipeline. It does not automatically receive exceptions from the executing wrapper; those follow the wrapper's own exception handling.

Definition validation can fail during registration. A validated patch can still fail during generation, for example when its selected operation is absent from the actual body. The transpiler reports instruction-matching and rewriting errors at that stage.

If an exception reaches the adapter's wrapper-generation call, `PatchDirectly` attempts to rebuild without Disharmony's transpiler while retaining other owners' patches. Deferred completion reports returned failures and continues through the remaining work. Exceptions from recovery or a handler propagate.

## Other parts of the repository

The core pipeline lives in the Disharmony library. Two additional projects connect it to the compiler and to RimWorld. The library also contains an optional optimizer after the rewriting steps.

### Analyzer

The analyzer checks patches while their source is being compiled. It uses Roslyn's compiler symbols and operations, whereas the runtime binder and registry inspect loaded methods through reflection. Those are separate representations, so the analyzer's checks are implemented independently.

[PatchAnalyzer.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony.Analyzers/PatchAnalyzer.cs) registers the checks; [DiagnosticGenerator.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony.Analyzers/DiagnosticGenerator.cs) implements them. A runtime validation change may need a corresponding analyzer change.

### RimWorld integration

The library provides deferred generation and error callbacks, but the host game determines when to complete pending work and where to report failures. The RimWorld project supplies those connections without making the library depend on game assemblies.

[DisharmonyMod.cs](https://github.com/RossM/Disharmony/blob/main/Disharmony.RimWorld/DisharmonyMod.cs) connects patch-generation errors to `Verse.Log` and schedules completion of deferred patches during startup.

### Optimizer

The transpiler has an optional optimization step after rule application and exception fixup. The [optimizer](https://github.com/RossM/Disharmony/blob/main/Disharmony/Optimizer/Optimizer.cs) represents the instructions as a control-flow graph: blocks of instructions connected by possible execution paths. Its passes track stack values, types, and variables through that graph.

The current implementation returns the incoming instruction list rather than emitting instructions from the graph, and its internal enable flag defaults to false. This explains why changes to the graph are not reflected in the returned IL.

## Glossary

These definitions are reminders for reading the code. The links lead to the explanations above.

| Term | Meaning in this guide |
| --- | --- |
| [Adapter](#install-the-harmony-wrapper) | The part of HarmonyInterface that handles a particular version of Harmony's internal APIs. |
| [Binding / ParameterBinding](#bindings-describe-value-sources) | Metadata describing where a patch parameter's value comes from and how it can be accessed. |
| [Detour](#architecture-at-a-glance) | A redirection from the original method to a replacement method. |
| [EmptyInvocation](#invocation-a-common-model-for-operations) | The invocation representing no inner target; used to distinguish whole-method patches. |
| [Evaluation stack](#invocation-a-common-model-for-operations) | The temporary stack through which IL instructions consume inputs and supply results. Separate from local-variable storage. |
| [Exception region](#keep-exception-regions-valid) | A portion of IL governed by exception handling, such as a try/catch block. |
| [IL](#architecture-at-a-glance) | Intermediate Language: the compiled .NET instructions rewritten by Disharmony and Harmony. |
| [Invocation](#invocation-a-common-model-for-operations) | An operation described by its inputs, result type, and instructions. It also supplies value equality for grouping equivalent operations. |
| [Local](#preserve-an-inner-operations-inputs-and-result) | Storage belonging to an execution of the generated method, used to hold inputs, results, state, and other temporary values. |
| [Method generation](#method-generation-create-the-replacement) | Applying a prepared ruleset to actual IL, then building and installing the executable replacement. |
| [Output template](#rulesets-describe-the-il-edits) | The instructions a rule emits, with local and label references mapped when the rule is applied. |
| [Owner ID](#install-a-trampoline) | Harmony's label identifying who registered a patch. Disharmony's transpiler entries share one owner ID. |
| [PatchInfo](#bindings-describe-value-sources) | The registry's validated internal patch description, including resolved bindings, invocations, options, and registration ownership. |
| [Pattern](#rulesets-describe-the-il-edits) | An instruction sequence used to locate edits in a method body. |
| [Phase](#apply-the-rules) | A rewriting pass whose output becomes the next phase's input. Rules in one phase match the same input. |
| [Placeholder](#rulesets-describe-the-il-edits) | A template reference to a local or branch label that is mapped to the generated method during rule application. |
| [Registration](#registration-prepare-the-changes) | Preparation of validated patch descriptions and rulesets, followed by installation of temporary forwarding methods. |
| [Resolver](#enter-through-a-trampoline) | The Disharmony callback that requests wrapper generation for a target reached through a trampoline. |
| [Rule](#rulesets-describe-the-il-edits) | A description of where an IL edit applies and which instructions to emit there. |
| [Ruleset](#rulesets-describe-the-il-edits) | The combined rules and shared placeholders used to rewrite one outer method. |
| [State key](#allocate-shared-state) | The identity used to make matching state bindings refer to the same generated local. |
| [Tail call](#enter-through-a-trampoline) | A final call emitted with IL's tail-call instruction, allowing the forwarding method's own call frame to be discarded. |
| [Trampoline](#enter-through-a-trampoline) | A temporary method that triggers generation of a replacement, then forwards its waiting call to that replacement. |
| [Transpiler](#architecture-at-a-glance) | Harmony's instruction-rewriting callback. Disharmony uses it to apply the outer method's ruleset. |
| [Wrapper](#install-the-harmony-wrapper) | Harmony's executable replacement combining the target body with its registered patches. |
