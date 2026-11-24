# Release checklist

Use this procedure for prereleases and final releases. Keep release-specific checkmarks and results in a working copy outside source control. Run each command separately from the repository root in PowerShell 7.4 or later, and stop if it fails.

Replace these placeholders before running commands:

| Placeholder | Value |
| --- | --- |
| `VERSION` | The chosen version, such as `1.0.0-beta.3` or `1.0.0`. |
| `BUILD_RUN_ID`, `PUBLISH_RUN_ID` | The numeric IDs of the corresponding GitHub Actions runs. |
| `RIMWORLD` | The game directory, normally `C:/Program Files (x86)/Steam/steamapps/common/RimWorld`. |
| `STEAM_LIBRARY` | The Steam library containing RimWorld's Workshop downloads, normally `C:/Program Files (x86)/Steam`. |

For the RimWorld build, set `WaylithCLRDirectory` in `Directory.Build.local.props` to the RimWorld 1.7 installation. The repository's SDK, Mono, and [GitHub CLI](https://cli.github.com/manual/) must be installed; GitHub authentication and NuGet trusted publishing must already be configured. The Workshop item is [3818084845](https://steamcommunity.com/sharedfiles/filedetails/?id=3818084845), and its upload symlink points to `Disharmony.RimWorld/bin/Release/Mod` in this checkout.

The [validation fixtures](validation/README.md) supply the package consumers. Recheck unchanged compiler-host and packaging setup only when changes affect it.

## 1. Prepare the source

- [ ] Merge the release changes into `main`. Set `Version` in `Directory.Build.props` and `<modVersion>` in [About.xml](Disharmony.RimWorld/About/About.xml) to the same release version, including any prerelease suffix. Confirm that version is absent from the [NuGet version list](https://www.nuget.org/packages/Disharmony#versions-body-tab).
- [ ] Move any diagnostic changes from [Unshipped](Disharmony.Analyzers/AnalyzerReleases.Unshipped.md) to [Shipped](Disharmony.Analyzers/AnalyzerReleases.Shipped.md), using a numeric release heading and a comment naming the first beta where applicable. Leave the files unchanged if there are no diagnostic changes.
- [ ] Update affected documentation and install-command versions in `Disharmony/README.md` and `Disharmony/PackageReadme.md`. If support changes, update `docs/compatibility.md`, `About.xml`, and `LoadFolders.xml`. For a final release, update beta wording on the landing pages.
- [ ] Verify that the package and mod versions both equal `VERSION`. Then create a new output directory.

```powershell
./scripts/Test-RimWorldModVersion.ps1 -ExpectedVersion VERSION
New-Item -ItemType Directory ./artifacts/release-VERSION
```

## 2. Test and tag

- [ ] Run the standard runtime and analyzer suites in Debug and Release. The runner checks the manifest version first, then tests CLR/net472 and Mono/net472 with Harmony 2 and CLR/net10.0 with Harmony 2 and 3. It stops on failure and retains logs and separate result files.

```powershell
./scripts/Test-ReleaseCandidate.ps1 -OutputDirectory ./artifacts/release-VERSION/tests
```

- [ ] Run the WaylithCLR suite separately in Debug and Release. In `Directory.Build.local.props`, set `WaylithCLRDirectory` to the RimWorld 1.7 installation and `HarmonyRuntimeILDirectory` to the Harmony mod's RimWorld 1.7 `Assemblies` directory. See [the example configuration](Directory.Build.local.example.props). Confirm that both runs execute tests and pass the WaylithCLR backend selection test.

```powershell
dotnet test ./Disharmony.Tests.WaylithCLR/Disharmony.Tests.WaylithCLR.csproj --configuration Debug --logger trx --results-directory ./artifacts/release-VERSION/tests/Debug/waylith
dotnet test ./Disharmony.Tests.WaylithCLR/Disharmony.Tests.WaylithCLR.csproj --configuration Release --logger trx --results-directory ./artifacts/release-VERSION/tests/Release/waylith
```

- [ ] Generate the HTML and Workshop description. Review changed HTML pages and `artifacts/workshop-description.txt` for stale version/support information and broken links. For a browser preview, add `-Serve` to the docs command.

```powershell
./docs/Build-Docs.ps1
./scripts/Build-WorkshopDescription.ps1
```

- [ ] Review and commit the release changes, then push `main`. After committing, `git status --short` must be empty. Wait for the Tests and HTML documentation runs for that commit to succeed; local tests above also cover Harmony 3.

```powershell
git diff --check
git status --short
git push origin main
gh run list --branch main --event push --limit 10
```

- [ ] Tag the accepted commit and retain its identity.

```powershell
git tag -a vVERSION -m "Disharmony VERSION"
git push origin vVERSION
git rev-parse "vVERSION^{commit}" | Set-Content ./artifacts/release-VERSION/commit.txt
```

## 3. Build and validate the artifacts

- [ ] Start a build-only NuGet run from `main`. Copy the new run's ID into `BUILD_RUN_ID`, wait for success, and download its packages. Its Build summary must name `vVERSION` and the commit in `commit.txt`.

```powershell
gh workflow run publish-nuget.yml --ref main -f tag=vVERSION -f publish=false
gh run list --workflow publish-nuget.yml --branch main --event workflow_dispatch --limit 5
gh run watch BUILD_RUN_ID --exit-status
gh run download BUILD_RUN_ID --name nuget-release --dir ./artifacts/release-VERSION/build-only
```

- [ ] Create a clean worktree at the tag for the validation scripts. In the main checkout, close RimWorld and make a **clean Release build of `Disharmony.RimWorld` from the release commit**. The mod build can run while CI checks that commit. Verify it is the commit that receives the tag, and use the exact build output for packaging, in-game validation, and publication. The packaging script reads the existing output, checks the Disharmony assemblies, including the 1.7 WaylithCLR backend, against the local Release binaries, and verifies the extracted archive.

  Builds that bundle Disharmony from this checkout, such as XylXenos, copy the DLLs returned by `GetRimWorldModAssemblies` from `Disharmony.RimWorld/bin/Release/<target-framework>`. A release built in another worktree can pass validation while those local DLLs are still stale. The clean build refreshes the files downstream projects bundle, and the hash checks verify that the Workshop candidate contains those same files.

```powershell
git worktree add --detach ./artifacts/release-VERSION/source vVERSION
dotnet clean ./Disharmony.RimWorld/Disharmony.RimWorld.csproj --configuration Release
dotnet build ./Disharmony.RimWorld/Disharmony.RimWorld.csproj --configuration Release
./scripts/Build-RimWorldRelease.ps1 -Version VERSION -OutputDirectory ./artifacts/release-VERSION/mod
```

- [ ] Run the checked-in consumer against the workflow package. Expect four passing runtime/Harmony combinations and a matching package payload; the runner retains its log and builds.

```powershell
& ./artifacts/release-VERSION/source/scripts/Test-NuGetPackage.ps1 -Version VERSION -PackageDirectory ./artifacts/release-VERSION/build-only -OutputDirectory ./artifacts/release-VERSION/build-only-validation
```

- [ ] Collect the three release assets: package, symbols, and the verified mod archive.

```powershell
New-Item -ItemType Directory ./artifacts/release-VERSION/assets
Copy-Item ./artifacts/release-VERSION/build-only/* ./artifacts/release-VERSION/assets
Copy-Item ./artifacts/release-VERSION/mod/Disharmony.RimWorld.VERSION.zip ./artifacts/release-VERSION/assets
```

## 4. Test the Workshop candidate

- [ ] Close RimWorld and verify the upload symlink points to this checkout's `Disharmony.RimWorld/bin/Release/Mod`.

```powershell
(Get-Item 'RIMWORLD/Mods/Disharmony').ResolveLinkTarget($true).FullName
```

- [ ] Compare the clean build's mod directory served by the upload symlink with the extracted archive. Keep this output fixed through in-game validation and Workshop publication.

```powershell
& ./artifacts/release-VERSION/source/scripts/Test-ReleaseFiles.ps1 -ExpectedDirectory ./artifacts/release-VERSION/mod/mod-upload -ActualDirectory ./Disharmony.RimWorld/bin/Release/Mod
```

- [ ] Check startup and colony loading in RimWorld 1.7 with Disharmony and XylXenos enabled. Repeat the check if a RimWorld 1.6 test installation is available. An isolated game installation can use a copy of the verified mod files. Inspect each run's log for new warnings or errors from these mods and retain it in the private release record. For a normal game launch, the log is `%USERPROFILE%/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Player.log`. If only 1.7 is available, confirm with the release author that the automated Mono/Harmony 2 tests and mod archive checks are sufficient for this release. Record the checks that ran for 1.6.
- [ ] Close the game and repeat the file comparison above. Keep the upload files fixed through publication.
- [ ] Create a draft GitHub release with the three assets. Save any initial notes for the release author to review in `artifacts/release-VERSION/release-notes.md`, which Git ignores. For a blank draft, replace `--notes-file` and its path with `--notes=`. Omit `--prerelease` for a final release.

```powershell
gh release create vVERSION --verify-tag --draft --prerelease --title 'Disharmony VERSION' --notes-file ./artifacts/release-VERSION/release-notes.md ./artifacts/release-VERSION/assets/Disharmony.VERSION.nupkg ./artifacts/release-VERSION/assets/Disharmony.VERSION.snupkg ./artifacts/release-VERSION/assets/Disharmony.RimWorld.VERSION.zip
```

## 5. Update and verify Workshop

- [ ] Repeat the section 4 upload-file comparison. Through RimWorld, update item `3818084845` from the prepared upload directory. Paste `artifacts/release-VERSION/mod/workshop-description.txt` as its description. The release author writes the change notes directly in Steam Workshop.
- [ ] Confirm the Workshop page remains public, keeps the same item ID, and shows the new description/change notes, project link, and supported-version tags `1.6` and `1.7`.
- [ ] Wait for Steam to download the update, then compare the subscribed installation with the frozen archive verified in section 4. Every relative file path and SHA-256 hash must match.

```powershell
& ./artifacts/release-VERSION/source/scripts/Test-ReleaseFiles.ps1 -ExpectedDirectory ./artifacts/release-VERSION/mod/mod-upload -ActualDirectory 'STEAM_LIBRARY/steamapps/workshop/content/294100/3818084845'
```

- [ ] If the Workshop files differ, stop the release. Fix the release build or upload process and verify the subscribed files again. If the fix changes the candidate, rebuild and revalidate it before uploading again.

## 6. Publish and verify NuGet

- [ ] After the Workshop file comparison passes, dispatch the publishing run from `main`. This publishes to NuGet. Use its ID for `PUBLISH_RUN_ID`; the run must succeed and its Build summary must name the same tag/commit. Download this run's artifacts.

```powershell
gh workflow run publish-nuget.yml --ref main -f tag=vVERSION -f publish=true
gh run list --workflow publish-nuget.yml --branch main --event workflow_dispatch --limit 5
gh run watch PUBLISH_RUN_ID --exit-status
gh run download PUBLISH_RUN_ID --name nuget-release --dir ./artifacts/release-VERSION/published
```

- [ ] Validate the actual publishing artifact. It can differ from the earlier build; expect four passing consumer runs and a matching payload.

```powershell
& ./artifacts/release-VERSION/source/scripts/Test-NuGetPackage.ps1 -Version VERSION -PackageDirectory ./artifacts/release-VERSION/published -OutputDirectory ./artifacts/release-VERSION/published-validation
```

- [ ] Replace the draft's two package assets with this run's exact files and retain the frozen mod archive. Save the final hashes. Download the three assets from the GitHub draft and verify their filenames and hashes against the local assets.

```powershell
Copy-Item ./artifacts/release-VERSION/published/* ./artifacts/release-VERSION/assets -Force
gh release upload vVERSION ./artifacts/release-VERSION/assets/Disharmony.VERSION.nupkg ./artifacts/release-VERSION/assets/Disharmony.VERSION.snupkg --clobber
Get-ChildItem ./artifacts/release-VERSION/assets -File | Get-FileHash -Algorithm SHA256 | Export-Csv ./artifacts/release-VERSION/assets-sha256.csv -NoTypeInformation
gh release download vVERSION --dir ./artifacts/release-VERSION/draft-verification
& ./artifacts/release-VERSION/source/scripts/Test-ReleaseFiles.ps1 -ExpectedDirectory ./artifacts/release-VERSION/assets -ActualDirectory ./artifacts/release-VERSION/draft-verification
```

- [ ] Wait for this version to complete validation and appear on the [NuGet package page](https://www.nuget.org/packages/Disharmony). Run the public-feed check: all four consumers, signature verification, and comparison with the publishing artifact must pass. NuGet's added signature is excluded from the payload comparison.

```powershell
& ./artifacts/release-VERSION/source/scripts/Test-NuGetPackage.ps1 -Version VERSION -PackageDirectory ./artifacts/release-VERSION/published -OutputDirectory ./artifacts/release-VERSION/public-validation -PublicFeed
```

## 7. Publish GitHub

- [ ] The release author reviews any draft text and finalizes the release notes directly in GitHub. Publish the draft after the author confirms the final notes and the draft assets pass the comparison in section 6.

```powershell
gh release edit vVERSION --draft=false
```

- [ ] On the public GitHub release page, confirm tag `vVERSION`, the approved notes, the three asset downloads, and the prerelease label if applicable. Follow the Workshop link from the [HTML home page](https://rossm.github.io/Disharmony/). Verify `VERSION` on the [NuGet package page](https://www.nuget.org/packages/Disharmony).
- [ ] Optionally announce the release. The author writes the posts.

Retain the release output directory and private execution record. Do not move a published tag or reuse a published NuGet version; corrections require a new version.
