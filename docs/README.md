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

The output is in `docs/_site`. Generating the site locally does not publish it.

## Publish to GitHub Pages

The [HTML documentation workflow](../.github/workflows/docs.yml) builds the site with the same script and SDK selection used for local previews. Pull requests build the docs for review; pushes to `main` publish them to GitHub Pages. You can also run the workflow manually from the repository's Actions tab, selecting `main`.

To enable publishing for this repository:

1. Open **Settings > Pages** and set **Build and deployment > Source** to **GitHub Actions**.
2. Run **Actions > HTML documentation > Run workflow** on `main` once the workflow is committed and pushed.
3. Follow the URL shown by the **Publish to GitHub Pages** job. Check the home page, a guide, an API page, navigation, and search on the live site before linking readers to it.

The workflow uploads `docs/_site` as a Pages artifact. Generated HTML stays out of Git, and no separate publishing branch or personal access token is needed. The deployment job uses the `github-pages` environment and publishes only `main`; pull requests and manual runs on other branches cannot deploy.

DocFX uses relative links, so the same output works under the repository's Pages path and at the local preview root. Publishing deploys the current documentation from `main`. It does not publish NuGet or Workshop packages.

See [GitHub's custom Pages workflow documentation](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages) for deployment requirements and repository configuration.

## Edit the source files

| Content to change | Source |
| --- | --- |
| C# learning prerequisites | `docs/learning-csharp.md` |
| Runtime-patching introduction | `docs/runtime-patching.md` |
| Getting-started guide | `Disharmony/README.md` |
| RimWorld author setup and deployment | `Disharmony.RimWorld/README.md` |
| RimWorld modding advice for coding agents | `docs/rimworld-agent-advice.md` |
| Steam Workshop description | `Disharmony.RimWorld/About/README.md` |
| HTML player installation and troubleshooting | `docs/rimworld-players.md` |
| In-game mod-manager description | `<description>` in `Disharmony.RimWorld/About/About.xml` |
| Investigating game code and planning a patch | `docs/planning-a-game-patch.md` |
| Harmony comparison, coexistence, and migration | `docs/harmony-comparison.md` |
| Advanced usage examples | `docs/usage-examples.md` |
| Analyzer setup and configuration | `Disharmony.Analyzers/README.md` |
| Diagnostic explanations and corrections | `docs/analyzer-diagnostics.md` |
| Development and testing guide | `Disharmony.Tests/README.md` |
| API descriptions | C# XML documentation comments |
| HTML home page and audience routes | `docs/index.md` |
| Supported runtimes and Harmony versions | `docs/compatibility.md` |
| How to read extension-member signatures | `docs/extension-members.md` |
| Site navigation | `docs/toc.yml` |
| Collapsible inherited API members | `docs/template/public/main.js` and `docs/template/public/main.css` |

Do not edit `docs/api`, `docs/obj`, or `docs/_site`. They contain generated files and are ignored by Git. The build copies the repository guides and adapts their links for the HTML site. The Workshop description and HTML player guide have separate sources: edit `Disharmony.RimWorld/About/README.md` for Workshop and `docs/rimworld-players.md` for HTML. Keep their Disharmony-specific load-order and Harmony advice consistent. The HTML guide also contains general installation help.

## Prepare the Steam Workshop description

From the repository root, run:

```powershell
./scripts/Build-WorkshopDescription.ps1
```

Copy the contents of `artifacts/workshop-description.txt` into the Workshop description editor. The script converts `Disharmony.RimWorld/About/README.md` to [Steam's formatting syntax](https://steamcommunity.com/comment/Guide/formattinghelp), including headings, bold text, and absolute links to the project and issue tracker. Edit the Markdown source and rerun the script when changing the description; the output is generated and ignored by Git.

Preview the description in Steam before saving it. Check headings, bold text, load-order advice, and all links. `About.xml` remains the separate description shown in RimWorld's mod manager.

## API generation and diagnostics

The reference is extracted from the compiled `net10.0` assembly and its XML comments. This includes C# 14 extension-block members that the pinned DocFX version omits when reading the project directly. Extension properties still appear as `get_...` methods; [reading extension members](extension-members.md) explains their C# usage. Native support is tracked in [DocFX issue 11010](https://github.com/dotnet/docfx/issues/11010).

DocFX warnings, such as broken documentation links, fail the build. The script also checks representative extension members for silent extraction omissions. Library compiler warnings remain visible, including warnings about missing XML comments.
