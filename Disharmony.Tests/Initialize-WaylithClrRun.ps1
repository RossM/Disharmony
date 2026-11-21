[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory)]
    [string] $RuntimeDirectory,

    [Parameter(Mandatory)]
    [string] $RunDirectory,

    [string] $AssemblyName = 'Disharmony.Tests'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$runtimeManifestPath = Join-Path $RuntimeDirectory 'RimWorld.deps.json'
if (-not (Test-Path -LiteralPath $runtimeManifestPath -PathType Leaf))
{
    throw "RimWorld.deps.json was not found in $RuntimeDirectory. Use a RimWorld 1.7 installation or a copy of its WaylithCLR runtime files and dependency manifest."
}

# Use only the framework pack from RimWorld's self-contained deployment.
$runtimePackPattern = 'runtimepack.Microsoft.NETCore.App.Runtime.*'
$runtimeManifest = Get-Content -LiteralPath $runtimeManifestPath -Raw | ConvertFrom-Json
$runtimeTarget = $runtimeManifest.targets.PSObject.Properties[$runtimeManifest.runtimeTarget.name].Value
$runtimePack = $runtimeTarget.PSObject.Properties | Where-Object Name -like "$runtimePackPattern/10.*"
if (@($runtimePack).Count -ne 1)
{
    throw "Expected one .NET 10 runtime pack in $runtimeManifestPath."
}

$assetNames = @($runtimePack.Value.runtime.PSObject.Properties.Name) + @($runtimePack.Value.native.PSObject.Properties.Name)
$assetPaths = $assetNames | ForEach-Object { Join-Path $RuntimeDirectory $_ }
Copy-Item -LiteralPath $assetPaths -Destination $RunDirectory -Force

# Use the same self-contained runtime for the NUnitLite app and VSTest's testhost.
$testManifestPath = Join-Path $RunDirectory "$AssemblyName.deps.json"
$testManifest = Get-Content -LiteralPath $testManifestPath -Raw | ConvertFrom-Json
$testTarget = $testManifest.targets.PSObject.Properties[$testManifest.runtimeTarget.name].Value
# An incremental build can leave the manifest from the previous staging run in place.
foreach ($previousPack in @($testTarget.PSObject.Properties | Where-Object Name -like $runtimePackPattern))
{
    $testTarget.PSObject.Properties.Remove($previousPack.Name)
    $testManifest.libraries.PSObject.Properties.Remove($previousPack.Name)
}
$packLibrary = $runtimeManifest.libraries.PSObject.Properties[$runtimePack.Name].Value
$testTarget | Add-Member -NotePropertyName $runtimePack.Name -NotePropertyValue $runtimePack.Value
$testManifest.libraries | Add-Member -NotePropertyName $runtimePack.Name -NotePropertyValue $packLibrary
[IO.File]::WriteAllText($testManifestPath, ($testManifest | ConvertTo-Json -Depth 20))

$testConfigPath = Join-Path $RunDirectory "$AssemblyName.runtimeconfig.json"
$testConfig = Get-Content -LiteralPath $testConfigPath -Raw | ConvertFrom-Json
$testConfig.runtimeOptions.PSObject.Properties.Remove('framework')
[IO.File]::WriteAllText($testConfigPath, ($testConfig | ConvertTo-Json -Depth 20))
