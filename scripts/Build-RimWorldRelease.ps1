#Requires -Version 7.4

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('\A[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?\z')]
    [string] $Version,

    [Parameter(Mandatory)]
    [string] $OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$buildDirectory = Join-Path $repositoryRoot 'Disharmony.RimWorld/bin/Release'
$mod = Join-Path $buildDirectory 'Mod'
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'Use a new mod release output directory.' }
$packageVersion = dotnet msbuild (Join-Path $repositoryRoot 'Disharmony/Disharmony.csproj') -nologo -getProperty:PackageVersion
if ($LASTEXITCODE -ne 0) { throw 'Could not read PackageVersion.' }
if ($packageVersion.Trim() -ne $Version) { throw "Source version $packageVersion does not match requested version $Version." }
New-Item -ItemType Directory -Path $output | Out-Null

Start-Transcript -Path (Join-Path $output 'build.log') | Out-Null
Push-Location $repositoryRoot
try
{
    if (!(Test-Path -LiteralPath $mod -PathType Container)) {
        throw "Missing Release output at $mod. Run a clean Release build of Disharmony.RimWorld in this checkout first."
    }
    $expectedRoots = @('About', 'LoadFolders.xml', 'LICENSE', 'THIRD-PARTY-NOTICES.txt', '1.6', '1.7')
    $actualRoots = @(Get-ChildItem -LiteralPath $mod -Force | ForEach-Object Name)
    if (Compare-Object $expectedRoots $actualRoots) { throw 'The mod root has missing or unexpected entries.' }
    & (Join-Path $PSScriptRoot 'Test-ReleaseFiles.ps1') -ExpectedDirectory (Join-Path $repositoryRoot 'Disharmony.RimWorld/About') -ActualDirectory (Join-Path $mod 'About')
    $targetFrameworks = [ordered]@{ '1.6' = 'net472'; '1.7' = 'net10.0' }
    foreach ($gameVersion in $targetFrameworks.Keys)
    {
        $assemblies = Join-Path $mod "$gameVersion/Assemblies"
        $expectedAssemblies = @('0Harmony.dll', 'Disharmony.dll', 'Disharmony.RimWorld.dll')
        $actualAssemblies = @(Get-ChildItem -LiteralPath $assemblies -Force | ForEach-Object Name)
        if (Compare-Object $expectedAssemblies $actualAssemblies) { throw "Unexpected assemblies for RimWorld $gameVersion." }
        $expectedMajor = if ($gameVersion -eq '1.6') { 2 } else { 3 }
        $harmonyVersion = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $assemblies '0Harmony.dll')).Version
        if ($harmonyVersion.Major -ne $expectedMajor) { throw "RimWorld $gameVersion requires Harmony $expectedMajor; found $harmonyVersion." }

        $framework = $targetFrameworks[$gameVersion]
        foreach ($name in 'Disharmony.dll', 'Disharmony.RimWorld.dll')
        {
            $builtPath = Join-Path $buildDirectory "$framework/$name"
            $modPath = Join-Path $assemblies $name
            if (!(Test-Path -LiteralPath $builtPath -PathType Leaf)) {
                throw "Missing Release binary at $builtPath."
            }
            $builtVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($builtPath).ProductVersion
            if (($builtVersion -split '\+', 2)[0] -ne $Version) {
                throw "$builtPath has product version '$builtVersion'. Expected $Version."
            }
            if ((Get-FileHash -LiteralPath $builtPath).Hash -ne (Get-FileHash -LiteralPath $modPath).Hash) {
                throw "$modPath differs from the Release build output at $builtPath."
            }
        }
        Write-Host "PASS: RimWorld $gameVersion packages the local Disharmony $Version Release binaries and Harmony $harmonyVersion."
    }

    & (Join-Path $PSScriptRoot 'Build-WorkshopDescription.ps1')
    Copy-Item -LiteralPath (Join-Path $repositoryRoot 'artifacts/workshop-description.txt') -Destination (Join-Path $output 'workshop-description.txt')

    $archive = Join-Path $output "Disharmony.RimWorld.$Version.zip"
    $upload = Join-Path $output 'mod-upload'
    Compress-Archive -Path (Join-Path $mod '*') -DestinationPath $archive
    Expand-Archive -LiteralPath $archive -DestinationPath $upload
    & (Join-Path $PSScriptRoot 'Test-ReleaseFiles.ps1') -ExpectedDirectory $mod -ActualDirectory $upload
    Write-Host "PASS: Mod archive and extracted upload files prepared in $output."
}
finally
{
    Pop-Location
    Stop-Transcript | Out-Null
}
