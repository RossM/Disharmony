[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('\Av(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)(?:-[0-9A-Za-z]+(?:[.-][0-9A-Za-z]+)*)?\z')]
    [string] $Tag,

    [string] $OutputDirectory = 'artifacts/nuget'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Call from a clean checkout of the main commit that dispatched the workflow.
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect the checkout.' }
if ($changes.Count -ne 0) { throw 'Release packaging requires a clean checkout.' }

$commit = git rev-parse --verify "refs/tags/$Tag^{commit}"
if ($LASTEXITCODE -ne 0) { throw "Release tag $Tag does not exist." }
git merge-base --is-ancestor $commit HEAD
if ($LASTEXITCODE -ne 0) { throw 'The release tag must belong to the dispatched main history.' }

$outputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
if (Test-Path -LiteralPath $outputPath) { throw 'Use a new output directory for release packages.' }

git checkout --detach $commit
if ($LASTEXITCODE -ne 0) { throw 'Could not check out the release tag.' }

$version = $Tag.Substring(1)
$packageVersion = dotnet msbuild ./Disharmony/Disharmony.csproj -nologo -getProperty:PackageVersion
if ($LASTEXITCODE -ne 0) { throw 'Could not read the package version.' }
if ($packageVersion.Trim() -cne $version) {
    throw "Tag $Tag does not match package version $packageVersion. Update the version before tagging."
}

dotnet pack ./Disharmony/Disharmony.csproj --configuration Release --output $outputPath -p:ContinuousIntegrationBuild=true
if ($LASTEXITCODE -ne 0) { throw 'Release packaging failed.' }

$files = @(Get-ChildItem -LiteralPath $outputPath -Force -Recurse -File)
if ($files.Count -ne 2) { throw 'Expected exactly one package and one symbol package.' }
$hashes = @{}
foreach ($extension in @('nupkg', 'snupkg')) {
    $path = Join-Path $outputPath "Disharmony.$version.$extension"
    $archive = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $nuspec = $archive.GetEntry('Disharmony.nuspec')
        if ($null -eq $nuspec) { throw "Missing package metadata in $extension." }
        $reader = [IO.StreamReader]::new($nuspec.Open())
        try { [xml] $document = $reader.ReadToEnd() }
        finally { $reader.Dispose() }
        $metadata = $document.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
        if ($metadata.id -cne 'Disharmony' -or $metadata.version -cne $version) {
            throw "Unexpected package identity in $extension."
        }
        if ($metadata.repository.commit -cne $commit) {
            throw "The $extension package does not identify the tagged source commit."
        }
    }
    finally { $archive.Dispose() }
    $hashes[$extension] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
}

if ($env:GITHUB_OUTPUT) {
    @(
        "version=$version"
        "package-sha256=$($hashes['nupkg'])"
        "symbols-sha256=$($hashes['snupkg'])"
    ) | Out-File -LiteralPath $env:GITHUB_OUTPUT -Encoding utf8 -Append
}
if ($env:GITHUB_STEP_SUMMARY) {
    @(
        '### NuGet release packages'
        ''
        "Tag: $Tag"
        "Commit: $commit"
        "Package: Disharmony $version"
        ''
        '| File | SHA-256 |'
        '| --- | --- |'
        "| Disharmony.$version.nupkg | $($hashes['nupkg']) |"
        "| Disharmony.$version.snupkg | $($hashes['snupkg']) |"
        ''
    ) | Out-File -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8 -Append
}
