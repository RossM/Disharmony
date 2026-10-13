# Release checklist

Use this procedure for beta and final releases. Keep release-specific checkmarks and results in a working copy outside source control. Run each command separately from the repository root in PowerShell 7.4 or later, and stop if it fails.

Replace these placeholders before running commands:

| Placeholder | Value |
| --- | --- |
| `VERSION` | The chosen version, such as `1.0.0-beta.3` or `1.0.0`. |
| `BUILD_RUN_ID`, `PUBLISH_RUN_ID` | The numeric IDs of the corresponding GitHub Actions runs. |
| `RIMWORLD` | The game directory, normally `C:/Program Files (x86)/Steam/steamapps/common/RimWorld`. |
| `STEAM_LIBRARY` | The Steam library containing RimWorld's Workshop downloads, normally `C:/Program Files (x86)/Steam`. |

The repository's SDK, Mono, and [GitHub CLI](https://cli.github.com/manual/) must be installed; GitHub authentication and NuGet trusted publishing must already be configured. The Workshop item is [3818084845](https://steamcommunity.com/sharedfiles/filedetails/?id=3818084845), and its upload symlink points to `Disharmony.RimWorld/bin/Release/Mod` in this checkout.

The [validation fixtures](validation/README.md) supply the package consumers. Recheck unchanged compiler-host and packaging setup only when changes affect it.

## 1. Prepare the source

- [ ] Merge the release changes into `main` and set `Version` in `Directory.Build.props`. Confirm that version is absent from the [NuGet version list](https://www.nuget.org/packages/Disharmony#versions-body-tab).
- [ ] Move any diagnostic changes from [Unshipped](Disharmony.Analyzers/AnalyzerReleases.Unshipped.md) to [Shipped](Disharmony.Analyzers/AnalyzerReleases.Shipped.md), using a numeric release heading and a comment naming the first beta where applicable. Leave the files unchanged if there are no diagnostic changes.
- [ ] Update [release-notes.md](release-notes.md) for this version. It supplies the GitHub notes and Workshop change notes.
- [ ] Update affected documentation and install-command versions in `Disharmony/README.md` and `Disharmony/PackageReadme.md`. If support changes, update `docs/compatibility.md`, `About.xml`, and `LoadFolders.xml`. For a final release, update beta wording on the landing pages.
- [ ] Confirm the package version and create a new output directory. The version printed below must equal `VERSION`; the output directory must not already exist.

```powershell
dotnet msbuild ./Disharmony/Disharmony.csproj -nologo -getProperty:PackageVersion
New-Item -ItemType Directory ./artifacts/release-VERSION
```

## 2. Test and tag

- [ ] Run all runtime and analyzer suites in Debug and Release. The runner covers CLR/net472 and Mono/net472 with Harmony 2, plus CLR/net10.0 with Harmony 2 and 3. It stops on failure and retains logs and separate result files.

```powershell
./scripts/Test-ReleaseCandidate.ps1 -OutputDirectory ./artifacts/release-VERSION/tests
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

- [ ] Create a clean worktree at the tag, then build the mod release from it. The script builds both frameworks, checks the folder layout and bundled Harmony majors, generates the Workshop description, and creates and verifies the mod archive and extracted upload files.

```powershell
git worktree add --detach ./artifacts/release-VERSION/source vVERSION
& ./artifacts/release-VERSION/source/scripts/Build-RimWorldRelease.ps1 -Version VERSION -OutputDirectory ./artifacts/release-VERSION/mod
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

- [ ] Close RimWorld. Resolve the game's upload symlink; it must point to this checkout's `Disharmony.RimWorld/bin/Release/Mod` before copying.

```powershell
(Get-Item 'RIMWORLD/Mods/Disharmony').ResolveLinkTarget($true).FullName
```

- [ ] Mirror the extracted archive into the upload directory and compare the files. Robocopy replaces the directory's contents, removing obsolete files; exit codes 0–7 are successful, while 8 or higher means stop. The file comparison must pass.

```powershell
robocopy ./artifacts/release-VERSION/mod/mod-upload ./Disharmony.RimWorld/bin/Release/Mod /MIR /R:1 /W:1
Write-Output $LASTEXITCODE
& ./artifacts/release-VERSION/source/scripts/Test-ReleaseFiles.ps1 -ExpectedDirectory ./artifacts/release-VERSION/mod/mod-upload -ActualDirectory ./Disharmony.RimWorld/bin/Release/Mod
```

- [ ] Launch RimWorld 1.6 with Disharmony and XylXenos enabled, then repeat with 1.7. Confirm startup and loading a colony succeed. After each run, inspect `%USERPROFILE%/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Player.log` for new warnings/errors from these mods and retain the log in the private release record.
- [ ] Close the game and repeat the file comparison above. Keep the upload files fixed through publication.
- [ ] Create a draft GitHub release with the tagged notes and three assets. The example is for a beta; omit `--prerelease` for a final release.

```powershell
gh release create vVERSION --verify-tag --draft --prerelease --title 'Disharmony VERSION' --notes-file ./artifacts/release-VERSION/source/release-notes.md ./artifacts/release-VERSION/assets/Disharmony.VERSION.nupkg ./artifacts/release-VERSION/assets/Disharmony.VERSION.snupkg ./artifacts/release-VERSION/assets/Disharmony.RimWorld.VERSION.zip
```

## 5. Publish and verify NuGet

- [ ] Dispatch the publishing run from `main`. This publishes to NuGet. Use its ID for `PUBLISH_RUN_ID`; the run must succeed and its Build summary must name the same tag/commit. Download this run's artifacts.

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

- [ ] Replace the draft's two package assets with this run's exact files; retain the frozen mod archive. Save the final asset hashes.

```powershell
Copy-Item ./artifacts/release-VERSION/published/* ./artifacts/release-VERSION/assets -Force
gh release upload vVERSION ./artifacts/release-VERSION/assets/Disharmony.VERSION.nupkg ./artifacts/release-VERSION/assets/Disharmony.VERSION.snupkg --clobber
Get-ChildItem ./artifacts/release-VERSION/assets -File | Get-FileHash -Algorithm SHA256 | Export-Csv ./artifacts/release-VERSION/assets-sha256.csv -NoTypeInformation
```

- [ ] Wait for this version to complete validation and appear on the [NuGet package page](https://www.nuget.org/packages/Disharmony). Run the public-feed check: all four consumers, signature verification, and comparison with the publishing artifact must pass. NuGet's added signature is excluded from the payload comparison.

```powershell
& ./artifacts/release-VERSION/source/scripts/Test-NuGetPackage.ps1 -Version VERSION -PackageDirectory ./artifacts/release-VERSION/published -OutputDirectory ./artifacts/release-VERSION/public-validation -PublicFeed
```

## 6. Update Workshop and publish GitHub

- [ ] Repeat the section 4 upload-file comparison. Through RimWorld, update item `3818084845` from the prepared upload directory. Paste `artifacts/release-VERSION/mod/workshop-description.txt` as its description and use the tagged `release-notes.md` for change notes.
- [ ] Confirm the Workshop page remains public, keeps the same item ID, and shows the new description/change notes, project link, and supported-version tags `1.6` and `1.7`.
- [ ] Wait for Steam to download the update, then compare the subscribed installation with the archive. The comparison must pass.

```powershell
& ./artifacts/release-VERSION/source/scripts/Test-ReleaseFiles.ps1 -ExpectedDirectory ./artifacts/release-VERSION/mod/mod-upload -ActualDirectory 'STEAM_LIBRARY/steamapps/workshop/content/294100/3818084845'
```

- [ ] Close RimWorld and temporarily move the development symlink outside `Mods`, so the subscribed copy is used. The destination must not already exist. Repeat the section 4 in-game checks on both game versions using the Workshop copy, then close the game and restore the symlink.

```powershell
Move-Item -LiteralPath 'RIMWORLD/Mods/Disharmony' -Destination 'RIMWORLD/Disharmony-development-link'
```

After the subscribed-install tests:

```powershell
Move-Item -LiteralPath 'RIMWORLD/Disharmony-development-link' -Destination 'RIMWORLD/Mods/Disharmony'
```

- [ ] Publish the GitHub draft, download its public assets, and compare them with the final local assets. All three filenames and hashes must match.

```powershell
gh release edit vVERSION --draft=false
gh release download vVERSION --dir ./artifacts/release-VERSION/public-release
& ./artifacts/release-VERSION/source/scripts/Test-ReleaseFiles.ps1 -ExpectedDirectory ./artifacts/release-VERSION/assets -ActualDirectory ./artifacts/release-VERSION/public-release
```

- [ ] On GitHub, confirm tag `vVERSION`, the approved notes, and the prerelease label for a beta. On the [HTML home page](https://rossm.github.io/Disharmony/), follow the NuGet and Workshop links and confirm they reach the published downloads.
- [ ] Optionally announce the release. The author writes the posts.

Retain the release output directory and private execution record. Do not move a published tag or reuse a published NuGet version; corrections require a new version.
