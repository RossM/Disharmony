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

Use the IDs in the [diagnostic index](../docs/analyzer-diagnostics.md#diagnostic-index) to configure individual rules. Microsoft documents [severity settings and configuration precedence](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options#severity-level) and [configuration-file placement and scope](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files). Suppressing an analyzer diagnostic does not change whether Disharmony can apply the patch at runtime.

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

Use the [diagnostic reference](../docs/analyzer-diagnostics.md) to look up an ID, understand its effect, and find a correction. It includes a searchable index with default severities, explanations of each rule, and examples for common mistakes.

<a id="diagnostic-details"></a>

## Contributing

For local-checkout analyzer references and analyzer tests, see [Development and testing](../Disharmony.Tests/README.md#work-on-the-analyzer).
