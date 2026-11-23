#Requires -Version 7.4

[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string] $ExpectedVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repositoryRoot
try {
    $packageVersion = dotnet msbuild ./Disharmony/Disharmony.csproj -nologo -getProperty:PackageVersion
    if ($LASTEXITCODE -ne 0) { throw 'Could not read PackageVersion.' }
    $packageVersion = $packageVersion.Trim()
    if ($ExpectedVersion -and $packageVersion -cne $ExpectedVersion) {
        throw "Source package version '$packageVersion' does not match requested version '$ExpectedVersion'."
    }

    $manifestPath = 'Disharmony.RimWorld/About/About.xml'
    $manifest = [xml] (Get-Content -LiteralPath $manifestPath -Raw)
    $versions = $manifest.SelectNodes('/ModMetaData/modVersion')
    if ($versions.Count -ne 1) {
        throw "Set exactly one <modVersion> in $manifestPath to '$packageVersion'."
    }
    $modVersion = $versions[0].InnerText
    if ($modVersion -cne $packageVersion) {
        throw "The <modVersion> in $manifestPath is '$modVersion'. Set it to package version '$packageVersion'."
    }

    Write-Host "PASS: RimWorld mod version and NuGet package version match ($packageVersion)."
}
finally {
    Pop-Location
}
