# Disharmony analyzers

An analyzer is a tool that checks your code and points out possible mistakes while you edit or build it. Disharmony's analyzer checks your patches, helping you find problems before starting the game.

**If you use the Disharmony NuGet package, the analyzer is already included.** There is nothing else to install or enable. See [Getting started](../Disharmony/README.md#write-your-first-patch) if you have not added the package yet.

<a id="use-the-analyzer"></a>

## Compiler and editor requirements

The analyzer runs inside your C# compiler, Roslyn. It requires **Roslyn 5.9 or newer**; the **.NET 10.0.400 SDK** meets this requirement. With an older compiler, modern SDKs skip the bundled analyzer, so you will not get Disharmony warnings or errors.

To see those messages while editing, your editor must also provide Roslyn 5.9 or newer. It may use a different compiler version from your command-line build.

These requirements apply to your build tools, independently of the game's runtime. You can use the analyzer while compiling a `net472` mod. It adds no runtime dependency to your mod.

## Check that analysis is running

Add this temporary class to a project that references Disharmony. It deliberately declares a prefix without `static`:

```csharp
using Disharmony;

[Patch(typeof(AnalyzerCheck))]
public class AnalyzerCheck
{
    public void TargetMethod() { }

    [Prefix]
    [Target(nameof(TargetMethod))]
    public void BeforeTarget() { }
}
```

Building should report **DISHARMONY0002** because `BeforeTarget` is not static. A compatible IDE should report it while editing too. No patch registration or game launch is needed. Remove the class after checking.

If the build reports the diagnostic but the editor does not, check the IDE's compiler compatibility and analyzer settings. If neither reports it, check the package reference and compiler requirements above, and whether analysis or this rule has been disabled. Microsoft explains how to [view and configure analyzer diagnostics in Visual Studio](https://learn.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=visualstudio#view-rule-violations).

## Configure diagnostics

Disharmony uses standard Roslyn severity settings and has no custom analyzer configuration options. For example, this `.editorconfig` entry treats writing to a patch parameter passed by value as an error:

```ini
[*.cs]
dotnet_diagnostic.DISHARMONY0031.severity = error
```

Use the IDs in the [diagnostic catalog](#diagnostics) to configure individual rules. Microsoft documents [severity settings and configuration precedence](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options#severity-level) and [configuration-file placement and scope](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files). Suppressing an analyzer diagnostic does not change whether Disharmony can apply the patch at runtime.

## Scope and limitations

The analyzer checks source code using Disharmony's built-in `[Prefix]` and `[Postfix]` attributes. Discovery warnings assume you register patch classes with `Patcher.PatchAll` or `Patcher.PatchCategory`. It checks:

- Patch declarations: whether methods are static, have an allowed return type, and have the attributes needed for discovery and targeting.
- Parameter bindings: conflicts and combinations that can be rejected without looking up the game's target method.
- Selected mistakes in patch bodies, such as assigning to a parameter passed by value or explicitly throwing from an `AlwaysRun` patch.

You still need to load and test your mod. The analyzer does not check:

- Whether a target or inner operation exists, whether a selector matches multiple members, or whether a game's update changed its signature.
- Whether a binding matches the actual target's arguments, result, instance, or fields, or whether a requested delegate matches the selected method.
- Whether patches apply successfully or produce the intended behavior together with other mods.

It does not analyze fluent `PatchConfig` registrations, patches declared through user-defined attribute subclasses, or generated code. Harmony-only patches are not checked against Disharmony's patch-method rules. See [patch registration](../Disharmony/Patcher.cs) for runtime validation and [usage examples](../docs/usage-examples.md) for working patches.

## Diagnostics

All listed rules are enabled by default. The severity column shows the setting before any project overrides.

| ID | Default severity | Diagnostic |
| --- | --- | --- |
| DISHARMONY0001 | Error | Patch method or an enclosing type has open generic parameters. |
| DISHARMONY0002 | Error | Patch method is not static. |
| DISHARMONY0003 | Error | Prefix does not return bool or void (by-ref bool is not bool). |
| DISHARMONY0004 | Error | Postfix does not return void. |
| DISHARMONY0005 | Error | Prefix with AlwaysRun does not return void. |
| DISHARMONY0006 | Warning | Prefix/postfix has no [Patch] or [HarmonyPatch] on its containing class. |
| DISHARMONY0007 | Warning | Prefix/postfix has no [Target] or [Targets] on the method or containing class. |
| DISHARMONY0008 | Warning | Method has a direct Disharmony attribute but no [Prefix] or [Postfix]. |
| DISHARMONY0009 | Error | Patch method has more than one prefix/postfix marker. |
| DISHARMONY0010 | Error | Patch method has more than one inner target selector. |
| DISHARMONY0011 | Error | Selector has no explicit/default declaring type or qualified member name. |
| DISHARMONY0012 | Warning | InnerConstant has a null value, which the registry does not support. |
| DISHARMONY0014 | Warning | Multiple patch markers or categories, including mixed Disharmony/Harmony attributes. |
| DISHARMONY0015 | Error | Target/inner selector lacks a member name without selecting a constructor. |
| DISHARMONY0016 | Error | Parameter has multiple binding attributes. |
| DISHARMONY0017 | Error | Parameter uses __caller or explicit Scope.Inner without an inner patch. |
| DISHARMONY0018 | Error | AlwaysRun prefix binds the return value. |
| DISHARMONY0019 | Error | Exception binding is not in an AlwaysRun postfix. |
| DISHARMONY0020 | Error | Method/base-method binding is not a concrete delegate passed by value. |
| DISHARMONY0021 | Error | Parameter type/modifier is incompatible with an exception, argument array, or known inner constant result. |
| DISHARMONY0022 | Error | Parameters sharing a state key in a patch class have incompatible types. |
| DISHARMONY0024 | Error | Parameter requests an instance, argument, or field from an inner constant. |
| DISHARMONY0025 | Warning | Prefix binds the result but returns void, so it cannot skip the target. |
| DISHARMONY0027 | Error | Parameter starts with __ but is not a recognized special name or ___field binding, and has no explicit binding attribute. |
| DISHARMONY0028 | Warning | Multiple parameters in one patch bind the same value. |
| DISHARMONY0029 | Warning | State key has no ref/out binding in any patch declared in the same class. |
| DISHARMONY0030 | Warning | State key is only bound through out parameters in its patch class. |
| DISHARMONY0031 | Warning | Patch writes to a parameter that is not ref/out. |
| DISHARMONY0032 | Warning | AlwaysRun patch contains an explicit throw or rethrow. |
| DISHARMONY0034 | Warning | Prefix result binding is ref rather than out. |
| DISHARMONY0035 | Warning | Postfix argument binding is out. |
| DISHARMONY0036 | Error | Arguments or MemberInfo binding is ref/out/in. |
| DISHARMONY0037 | Error | Inner constant cannot supply member metadata or a base method. |
| DISHARMONY0038 | Error | MemberInfo parameter cannot accept any supported member metadata type. |

## Diagnostic details

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

## Contributing

For local-checkout analyzer references and analyzer tests, see [Development and testing](../Disharmony.Tests/README.md#work-on-the-analyzer).
