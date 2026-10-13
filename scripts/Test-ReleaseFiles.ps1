#Requires -Version 7.4

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ExpectedDirectory,

    [Parameter(Mandatory)]
    [string] $ActualDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$manifests = @()
foreach ($directory in @($ExpectedDirectory, $ActualDirectory))
{
    $root = (Resolve-Path -LiteralPath $directory).Path
    if (-not (Test-Path -LiteralPath $root -PathType Container)) { throw "Not a directory: $root" }
    $manifest = @(Get-ChildItem -LiteralPath $root -Recurse -Force -File | ForEach-Object {
        [pscustomobject]@{
            Path = [IO.Path]::GetRelativePath($root, $_.FullName)
            SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
        }
    })
    if ($manifest.Count -eq 0) { throw "No files found in $root" }
    $manifests += ,$manifest
}
$differences = @(Compare-Object -ReferenceObject $manifests[0] -DifferenceObject $manifests[1] -Property Path, SHA256)
if ($differences.Count)
{
    $differences | Format-Table | Out-Host
    throw 'Release directories differ in their file lists or SHA-256 hashes.'
}
Write-Host "PASS: $($manifests[0].Count) files match by relative path and SHA-256 hash."
