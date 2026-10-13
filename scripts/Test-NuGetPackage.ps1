#Requires -Version 7.4

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('\A[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?\z')]
    [string] $Version,

    [Parameter(Mandatory)]
    [string] $PackageDirectory,

    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [switch] $PublicFeed,

    [string] $MonoExecutable
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$template = Join-Path $repositoryRoot 'validation/NuGetConsumer'
$package = Join-Path (Resolve-Path -LiteralPath $PackageDirectory).Path "Disharmony.$Version.nupkg"
if (-not (Test-Path -LiteralPath $package -PathType Leaf)) { throw "Missing package: $package" }
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'Use a new validation output directory.' }

if (-not $MonoExecutable)
{
    $mono = Get-Command mono -CommandType Application -ErrorAction SilentlyContinue
    $MonoExecutable = if ($mono) { $mono.Source } else { Join-Path $env:ProgramFiles 'Mono/bin/mono.exe' }
}
$MonoExecutable = (Resolve-Path -LiteralPath $MonoExecutable).Path
$harmonyV3Version = dotnet msbuild (Join-Path $repositoryRoot 'Disharmony.Tests/Disharmony.Tests.csproj') -nologo -getProperty:HarmonyV3Version
if ($LASTEXITCODE -ne 0) { throw 'Could not read HarmonyV3Version.' }
$harmonyV3Version = $harmonyV3Version.Trim()

New-Item -ItemType Directory -Path $output | Out-Null
$configName = if ($PublicFeed) { 'NuGet.Public.config' } else { 'NuGet.Local.config' }
$config = Join-Path $output 'NuGet.Config'
Copy-Item -LiteralPath (Join-Path $template $configName) -Destination $config
if (-not $PublicFeed)
{
    $feed = New-Item -ItemType Directory -Path (Join-Path $output 'feed')
    Copy-Item -LiteralPath $package -Destination $feed.FullName
}
$cache = Join-Path $output 'packages'
$cases = @(
    @{ Name = 'clr-net472'; Framework = 'net472'; Runtime = 'Clr'; HarmonyVersion = ''; Major = '2' }
    @{ Name = 'mono-net472'; Framework = 'net472'; Runtime = 'Mono'; HarmonyVersion = ''; Major = '2' }
    @{ Name = 'net10-harmony2'; Framework = 'net10.0'; Runtime = 'Dotnet'; HarmonyVersion = ''; Major = '2' }
    @{ Name = 'net10-harmony3'; Framework = 'net10.0'; Runtime = 'Dotnet'; HarmonyVersion = $harmonyV3Version; Major = '3' }
)

Start-Transcript -Path (Join-Path $output 'validation.log') | Out-Null
try
{
    foreach ($case in $cases)
    {
        Write-Host "Validating $($case.Name)..."
        $consumer = New-Item -ItemType Directory -Path (Join-Path $output $case.Name)
        Copy-Item -LiteralPath (Join-Path $template 'NuGetConsumer.csproj'), (Join-Path $template 'Program.cs') -Destination $consumer.FullName
        $project = Join-Path $consumer.FullName 'NuGetConsumer.csproj'
        $properties = @(
            '-p:ImportDirectoryBuildProps=false', '-p:ImportDirectoryBuildTargets=false',
            "-p:DisharmonyPackageVersion=$Version", "-p:TargetFramework=$($case.Framework)"
        )
        if ($case.HarmonyVersion) { $properties += "-p:HarmonyVersion=$($case.HarmonyVersion)" }
        dotnet restore $project --configfile $config --packages $cache @properties
        if ($LASTEXITCODE -ne 0) { throw "Restore failed: $($case.Name)." }
        dotnet build $project --no-restore --configuration Release @properties
        if ($LASTEXITCODE -ne 0) { throw "Build failed: $($case.Name)." }
        $binary = Join-Path $consumer.FullName "bin/Release/$($case.Framework)/NuGetConsumer"
        switch ($case.Runtime)
        {
            'Clr' { & "$binary.exe" $Version $case.Major }
            'Mono' { & $MonoExecutable "$binary.exe" $Version $case.Major }
            'Dotnet' { dotnet "$binary.dll" $Version $case.Major }
        }
        if ($LASTEXITCODE -ne 0) { throw "Consumer failed: $($case.Name)." }
    }

    $restoredPackage = Join-Path $cache "disharmony/$Version/disharmony.$Version.nupkg"
    if ($PublicFeed)
    {
        dotnet nuget verify --all $restoredPackage
        if ($LASTEXITCODE -ne 0) { throw 'Published package signature verification failed.' }
    }
    # NuGet.org adds a signature; compare the contents rather than the signed archive bytes.
    $payloads = @()
    foreach ($path in @($package, $restoredPackage))
    {
        $archive = [IO.Compression.ZipFile]::OpenRead($path)
        try
        {
            $payload = @{}
            foreach ($entry in $archive.Entries)
            {
                if (-not $entry.Name -or $entry.FullName -eq '.signature.p7s') { continue }
                $stream = $entry.Open()
                try { $payload.Add($entry.FullName, [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))) }
                finally { $stream.Dispose() }
            }
            $payloads += ,$payload
        }
        finally { $archive.Dispose() }
    }
    if ($payloads[0].Count -ne $payloads[1].Count) { throw 'Restored package file list differs from the release artifact.' }
    foreach ($name in $payloads[0].Keys)
    {
        if ($payloads[0][$name] -cne $payloads[1][$name]) { throw "Restored package payload differs: $name" }
    }
    Write-Host 'PASS: four consumer runs and package payload comparison.'
}
finally { Stop-Transcript | Out-Null }
