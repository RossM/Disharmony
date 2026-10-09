# Building the HTML documentation

Use this guide when contributing to the documentation or previewing changes locally. You need PowerShell and the .NET SDK selected by the repository's `global.json` (10.0.400 or a later patch in the same feature band). The build script restores the pinned DocFX tool automatically.

Before changing documentation content, read [Writing Disharmony documentation](writing-documentation.md) for audience, page-purpose, and content-placement rules.

## Preview the docs

From the repository root, run:

```powershell
./docs/Build-Docs.ps1 -Serve
```

Open <http://localhost:8080> in your browser. The command builds the site and keeps the preview server running until you press Ctrl+C. Use `-Port 8081` if port 8080 is already in use.

After editing a source file, stop the server and run the command again to rebuild. Preview over HTTP rather than opening HTML files directly, so search and navigation can load their supporting files.

To generate HTML without starting a server:

```powershell
./docs/Build-Docs.ps1
```

The output is in `docs/_site`. It is a static site that can be hosted later; generating it does not publish it.

## Edit the source files

| Content to change | Source |
| --- | --- |
| C# learning prerequisites | `docs/learning-csharp.md` |
| Runtime-patching introduction | `docs/runtime-patching.md` |
| Getting-started guide | `Disharmony/README.md` |
| RimWorld author setup and deployment | `Disharmony.RimWorld/README.md` |
| Steam Workshop description | `Disharmony.RimWorld/About/README.md` |
| In-game mod-manager description | `<description>` in `Disharmony.RimWorld/About/About.xml` |
| Investigating game code and planning a patch | `docs/planning-a-game-patch.md` |
| Harmony comparison, coexistence, and migration | `docs/harmony-comparison.md` |
| Advanced usage examples | `docs/usage-examples.md` |
| Analyzer diagnostics | `Disharmony.Analyzers/README.md` |
| Development and testing guide | `Disharmony.Tests/README.md` |
| API descriptions | C# XML documentation comments |
| Overview | `docs/index.md` |
| Supported runtimes and Harmony versions | `docs/compatibility.md` |
| How to read extension-member signatures | `docs/extension-members.md` |
| Site navigation | `docs/toc.yml` |

Do not edit `docs/api`, `docs/obj`, or `docs/_site`. They contain generated files and are ignored by Git. The build copies the guide READMEs and adapts their source-code links for the HTML reference.

## API generation and diagnostics

The reference is extracted from the compiled `net10.0` assembly and its XML comments. This includes C# 14 extension-block members that the pinned DocFX version omits when reading the project directly. Extension properties still appear as `get_...` methods; [reading extension members](extension-members.md) explains their C# usage. Native support is tracked in [DocFX issue 11010](https://github.com/dotnet/docfx/issues/11010).

DocFX warnings, such as broken documentation links, fail the build. The script also checks representative extension members for silent extraction omissions. Library compiler warnings remain visible, including warnings about missing XML comments.
