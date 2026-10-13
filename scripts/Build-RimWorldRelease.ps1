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
$project = Join-Path $repositoryRoot 'Disharmony.RimWorld/Disharmony.RimWorld.csproj'
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
    # The outer build assembles the complete mod after both framework builds finish.
    dotnet build $project -c Release
    if ($LASTEXITCODE -ne 0) { throw "RimWorld mod build failed with exit code $LASTEXITCODE." }
    & (Join-Path $PSScriptRoot 'Build-WorkshopDescription.ps1')
    Copy-Item -LiteralPath (Join-Path $repositoryRoot 'artifacts/workshop-description.txt') -Destination (Join-Path $output 'workshop-description.txt')

    $mod = Join-Path $repositoryRoot 'Disharmony.RimWorld/bin/Release/Mod'
    $expectedRoots = @('About', 'LoadFolders.xml', 'LICENSE', 'THIRD-PARTY-NOTICES.txt', '1.6', '1.7')
    $actualRoots = @(Get-ChildItem -LiteralPath $mod -Force | ForEach-Object Name)
    if (Compare-Object $expectedRoots $actualRoots) { throw 'The mod root has missing or unexpected entries.' }
    & (Join-Path $PSScriptRoot 'Test-ReleaseFiles.ps1') -ExpectedDirectory (Join-Path $repositoryRoot 'Disharmony.RimWorld/About') -ActualDirectory (Join-Path $mod 'About')
    foreach ($gameVersion in @('1.6', '1.7'))
    {
        $assemblies = Join-Path $mod "$gameVersion/Assemblies"
        $expectedAssemblies = @('0Harmony.dll', 'Disharmony.dll', 'Disharmony.RimWorld.dll')
        $actualAssemblies = @(Get-ChildItem -LiteralPath $assemblies -Force | ForEach-Object Name)
        if (Compare-Object $expectedAssemblies $actualAssemblies) { throw "Unexpected assemblies for RimWorld $gameVersion." }
        $expectedMajor = if ($gameVersion -eq '1.6') { 2 } else { 3 }
        $harmonyVersion = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $assemblies '0Harmony.dll')).Version
        if ($harmonyVersion.Major -ne $expectedMajor) { throw "RimWorld $gameVersion requires Harmony $expectedMajor; found $harmonyVersion." }
        Write-Host "PASS: RimWorld $gameVersion bundles Harmony $harmonyVersion and the two Disharmony assemblies."
    }

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
