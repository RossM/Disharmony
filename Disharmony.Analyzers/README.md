# Disharmony analyzers

A build-time Roslyn analyzer for patch discovery, patch-method validation, and parameter bindings that do not require target reflection.
The analyzer targets .NET Standard 2.0 and does not load Disharmony, Harmony, or RimWorld at runtime.

| ID | Diagnostic |
| --- | --- |
| DISHARMONY0001 | Patch method or an enclosing type has open generic parameters. |
| DISHARMONY0002 | Patch method is not static. |
| DISHARMONY0003 | Prefix does not return bool or void (by-ref bool is not bool). |
| DISHARMONY0004 | Postfix does not return void. |
| DISHARMONY0005 | Prefix with AlwaysRun does not return void. |
| DISHARMONY0006 | Prefix/postfix has no [Patch] or [HarmonyPatch] on its containing class. |
| DISHARMONY0007 | Prefix/postfix has no [Target] or [Targets] on the method or containing class. |
| DISHARMONY0008 | Method has a direct Disharmony attribute but no [Prefix] or [Postfix]. |
| DISHARMONY0009 | Multiple patch type attributes would make SingleOrDefault throw. |
| DISHARMONY0010 | Multiple inner target attributes would make SingleOrDefault throw. |
| DISHARMONY0011 | Selector has no explicit/default declaring type or qualified member name. |
| DISHARMONY0012 | InnerConstant has a null value, which the registry does not support. |
| DISHARMONY0014 | Multiple patch markers or categories, including mixed Disharmony/Harmony attributes. |
| DISHARMONY0015 | Target/inner selector lacks a member name without selecting a constructor. |
| DISHARMONY0016 | Parameter has multiple binding attributes. |
| DISHARMONY0017 | Parameter uses __caller or explicit Scope.Inner without an inner patch. |
| DISHARMONY0018 | AlwaysRun prefix binds the return value. |
| DISHARMONY0019 | Exception binding is not in an AlwaysRun postfix. |
| DISHARMONY0020 | Method/base-method binding is not a concrete delegate passed by value. |
| DISHARMONY0021 | Parameter type/modifier is incompatible with an exception, argument array, or known inner constant result. |
| DISHARMONY0022 | Parameters sharing a state key in a patch class have incompatible types. |
| DISHARMONY0024 | Parameter requests an instance, argument, or field from an inner constant. |
| DISHARMONY0025 | Prefix binds the result but returns void, so it cannot skip the target. |
| DISHARMONY0027 | Parameter starts with __ but is not a recognized special name or ___field binding, and has no explicit binding attribute. |
| DISHARMONY0028 | Multiple parameters in one patch bind the same value. |
| DISHARMONY0029 | State key has no ref/out binding in any patch declared in the same class. |
| DISHARMONY0030 | State key is only bound through out parameters in its patch class. |
| DISHARMONY0031 | Patch writes to a parameter that is not ref/out. |
| DISHARMONY0032 | AlwaysRun patch contains an explicit throw or rethrow. |
| DISHARMONY0034 | Prefix result binding is ref rather than out. |
| DISHARMONY0035 | Postfix argument binding is out. |
| DISHARMONY0036 | Arguments or MemberInfo binding is ref/out/in. |
| DISHARMONY0037 | Inner constant cannot supply member metadata or a base method. |
| DISHARMONY0038 | MemberInfo parameter cannot accept any supported member metadata type. |

The analyzer assumes assembly discovery through Patcher.PatchAll or Patcher.PatchCategory.
Methods are identified by the built-in Disharmony Prefix/Postfix attributes. User-defined attribute subclasses are ignored.
Discovery markers and targets follow reflection inheritance and can be on another part of a partial class.
DISHARMONY0008 checks only attributes directly on the method, from the built-in set;
it does not flag helpers for inherited method attributes, class defaults, return attributes, or parameter bindings.
Method-level PatchOptions replace class-level options; options follow reflection inheritance from base classes, but not enclosing classes,
matching PatchRegistry.GetAttributes. DISHARMONY0005 takes precedence over DISHARMONY0003 for an AlwaysRun prefix.
Warnings point to the method, parameter, selector attribute, or discovery class and can be configured individually in .editorconfig.
Multiplicity checks follow the built-in attributes' inheritance and multiplicity rules, including suppression of overridden attributes.
Discovery checks treat [Patch]/[HarmonyPatch] as one group and [Category]/[HarmonyPatchCategory] as another.
Duplicate discovery warnings include effective inherited attributes, even when their values agree.
Duplicate categories also warn on classes without a discovery marker. Runtime precedence remains unchanged.
Class targets are added to method targets rather than replaced, so every effective selector is checked.

Qualified selectors such as Namespace.Type:Member and Namespace.Type.Member can resolve their declaring type at runtime.
DISHARMONY0011 accepts both forms without attempting that lookup. Inner selectors do not inherit the outer target's type.

This analyzer does not analyze programmatic PatchConfig registrations,
resolve target methods, validate target-dependent parameter bindings/signature filters, or enforce style.
Lookup-dependent failures (missing/ambiguous members, field-versus-method selection, target method restrictions,
and nested-member resolution) and patch application failures remain runtime checks.
Explicit built-in parameter binding attributes override reserved names.
Known-type checks follow the runtime's ref/in/out rules and AllowUnsafe reference-type bypass; delegate shape and state-type checks do not bypass validation.
State keys are compared within each declaring patch class across its declared patch methods, including methods with different targets.
DISHARMONY0029 warns when no patch in the declaring class binds a state key through ref or out; it checks declarations, not assignments in method bodies. Ref/in/out modifiers do not change the stored state type.
Ordinary argument index bounds, argument/result/instance/field type compatibility, delegate signatures, iterator-state-machine restrictions,
and writable-reference restrictions that depend on target parameters remain runtime checks.
Harmony-only patches are not subject to Disharmony's return-type rules.
Runtime validation remains necessary, including for methods registered through reflection or constructed generic types.

Reference this project from each project that contains patches (analyzer project references are not transitive):

```xml
<ProjectReference Include="..\Disharmony.Analyzers\Disharmony.Analyzers.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

Run tests with `dotnet test Disharmony.Analyzers.Tests/Disharmony.Analyzers.Tests.csproj`.
Tests use minimal attribute metadata and do not execute patches or load the game.

DISHARMONY0025 flags a void prefix binding __result or [ReturnValue]; DISHARMONY0018 takes precedence for AlwaysRun prefixes. DISHARMONY0031 warns when a patch writes to a parameter passed by value.

DISHARMONY0028 compares special bindings, state keys, argument names or indexes, fields, and method names with their scopes. Aliases requiring target reflection (such as an argument name and index) are not compared. On inner patches, named Scope.Any lookups retain their fallback semantics and are distinct from explicit Inner/Outer lookups.

DISHARMONY0030 checks state bindings across patches declared in the same class. Value, in, and ref parameters count as readers; out parameters do not. No method-body analysis is performed.

DISHARMONY0031 checks direct assignments (including compound, coalescing, and deconstruction assignments), increments/decrements, and passing a value parameter as ref/out. Captured patch parameters are checked in lambdas and local functions. Member/array-element writes and writes through local aliases are not analyzed.

DISHARMONY0032 checks throw statements and expressions directly in an AlwaysRun patch body, including rethrows. It does not follow calls or inspect nested lambda/local-function bodies, and does not determine whether a throw is caught locally.

DISHARMONY0035 warns when a postfix binds an argument through out, including implicit argument names and explicit [Argument] bindings. Read by value, in, or ref, or use a prefix to change arguments before the target runs. This does not apply to result, field, state, or other non-argument bindings.

The binding API uses [Argument] for individual arguments, [Arguments] or __args for an argument array, and [MemberInfo] for target metadata.
DISHARMONY0036 rejects ref/out/in on [Arguments] and [MemberInfo]. Argument arrays must accept object[]; AllowUnsafe does not bypass this check.
DISHARMONY0038 rejects [MemberInfo] parameter types that cannot accept any of FieldInfo, MethodInfo, or ConstructorInfo. Compatibility with the actual target remains a runtime check. AllowUnsafe bypasses reference-type compatibility; ref/out/in remain invalid.
DISHARMONY0037 rejects [MemberInfo] and [BaseMethod] when they select an inner constant. An argument array for a constant is valid and empty.
Explicit Scope.Inner still requires an inner patch.

Duplicate checks include effective scope for [Arguments], [MemberInfo], and [BaseMethod]. MemberInfo defaults to Outer; the others default to Any.
Field and method identities include the explicit declaring type. Method identities also include the delegate signature and virtualCall.
Qualified and unqualified names are not equated without resolving the member. Inner field bindings with explicit declaring types or qualified names
are not rejected solely because the inner target is a constant: they may resolve to static fields.