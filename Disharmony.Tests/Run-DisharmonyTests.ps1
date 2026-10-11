[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Clr', 'Mono', 'Dotnet')]
    [string] $Runtime = 'Mono',

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [string] $MonoExecutable,

    [ValidateSet('2', '3')]
    [string[]] $HarmonyVersion,

    [string] $HarmonyV3Path = (Join-Path $PSScriptRoot '..\TestArtifacts\Harmony\3.0.0-rc.2\net10.0\0Harmony.dll'),

    [switch] $Profile,

    [string] $MonoProfile,

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $NUnitArguments = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-MonoExecutable([string] $RequestedExecutable)
{
    if ($RequestedExecutable)
    {
        if (-not (Test-Path -LiteralPath $RequestedExecutable -PathType Leaf))
        {
            throw "The requested Mono executable does not exist: $RequestedExecutable"
        }

        return (Resolve-Path -LiteralPath $RequestedExecutable).Path
    }

    if ($env:MONO_EXE)
    {
        if (-not (Test-Path -LiteralPath $env:MONO_EXE -PathType Leaf))
        {
            throw "MONO_EXE does not identify an existing file: $env:MONO_EXE"
        }

        return (Resolve-Path -LiteralPath $env:MONO_EXE).Path
    }

    $command = Get-Command mono -CommandType Application -ErrorAction SilentlyContinue
    if ($command)
    {
        return $command.Source
    }

    $candidates = @(
        (Join-Path ([Environment]::GetFolderPath('ProgramFiles')) 'Mono\bin\mono.exe'),
        (Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Mono\bin\mono.exe')
    )
    foreach ($candidate in $candidates)
    {
        if ($candidate -and (Test-Path -LiteralPath $candidate -PathType Leaf))
        {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw 'Mono was not found. Install Mono or specify -MonoExecutable or MONO_EXE.'
}

if (($Profile -or $MonoProfile) -and $Runtime -ne 'Mono')
{
    throw 'Mono profiling options can only be used with -Runtime Mono.'
}

if ($Profile -and $MonoProfile)
{
    throw 'Use either -Profile or -MonoProfile, not both.'
}

if (-not $HarmonyVersion)
{
    # The supplied v3 beta targets .NET 10; CLR and Mono retain their v2 default.
    $HarmonyVersion = if ($Runtime -eq 'Dotnet') { @('2', '3') } else { @('2') }
}
$HarmonyVersion = @($HarmonyVersion | Select-Object -Unique)

if ('3' -in $HarmonyVersion)
{
    if (-not (Test-Path -LiteralPath $HarmonyV3Path -PathType Leaf))
    {
        throw "Harmony v3 was not found at $HarmonyV3Path. Supply -HarmonyV3Path or select -HarmonyVersion 2."
    }
    $HarmonyV3Path = (Resolve-Path -LiteralPath $HarmonyV3Path).Path
    if ([Reflection.AssemblyName]::GetAssemblyName($HarmonyV3Path).Version.Major -ne 3)
    {
        throw "The requested Harmony v3 assembly is not version 3: $HarmonyV3Path"
    }
}

if ($HarmonyVersion.Count -gt 1 -and ($NUnitArguments | Where-Object { $_ -match '^--result(?:=|$)' }))
{
    throw 'Use a single -HarmonyVersion with a custom --result path, or omit --result to save separate results for both versions.'
}
if ($HarmonyVersion.Count -gt 1 -and $MonoProfile -match '(^|,)output=')
{
    throw 'Use a single -HarmonyVersion with a custom Mono profile output path.'
}

$testProjectRoot = $PSScriptRoot
$repositoryRoot = Split-Path -Parent $testProjectRoot
$testProject = Join-Path $testProjectRoot 'Disharmony.Tests.csproj'
$targetFramework = if ($Runtime -eq 'Dotnet') { 'net10.0' } else { 'net472' }
$testFileName = if ($Runtime -eq 'Dotnet') { 'Disharmony.Tests.dll' } else { 'Disharmony.Tests.exe' }
$buildDirectory = Join-Path $testProjectRoot "bin\$Configuration\$targetFramework"
$dotnet = Get-Command dotnet -CommandType Application -ErrorAction Stop
$runnerExitCode = 0
$resultDirectory = Join-Path $repositoryRoot 'TestResults\Disharmony'
$previousExpectedMajor = $env:DISHARMONY_TEST_HARMONY_MAJOR

Push-Location $repositoryRoot
try
{
    Write-Host "Building Disharmony tests ($Configuration, $targetFramework)..."
    & $dotnet.Source build $testProject --configuration $Configuration --framework $targetFramework
    if ($LASTEXITCODE -ne 0)
    {
        throw "Building Disharmony tests failed with exit code $LASTEXITCODE."
    }

    if (-not (Test-Path -LiteralPath (Join-Path $buildDirectory $testFileName) -PathType Leaf))
    {
        throw "The test executable was not produced in $buildDirectory."
    }

    foreach ($version in $HarmonyVersion)
    {
        # Stage the same v2-compiled binaries for each run, replacing only the runtime Harmony DLL.
        # Keeping these directories outside the build output also keeps logs and dependencies separate.
        $runDirectory = Join-Path $testProjectRoot "bin\HarmonyCompatibility\$Configuration\$targetFramework\v$version"
        New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
        Get-ChildItem -LiteralPath $buildDirectory | Copy-Item -Destination $runDirectory -Recurse -Force
        if ($version -eq '3')
        {
            Copy-Item -LiteralPath $HarmonyV3Path -Destination (Join-Path $runDirectory '0Harmony.dll') -Force
        }

        $testExecutable = Join-Path $runDirectory $testFileName
        $env:DISHARMONY_TEST_HARMONY_MAJOR = $version
        $expectedProfilePath = $null
        $effectiveNUnitArguments = [Collections.Generic.List[string]]::new()
        foreach ($argument in $NUnitArguments)
        {
            $effectiveNUnitArguments.Add($argument)
        }
        if (-not ($NUnitArguments | Where-Object { $_ -match '^--result(?:=|$)' }))
        {
            New-Item -ItemType Directory -Path $resultDirectory -Force | Out-Null
            $resultPath = Join-Path $resultDirectory "test-result-$($Runtime.ToLowerInvariant())-harmony$version.xml"
            $effectiveNUnitArguments.Add("--result=$resultPath")
        }

        Write-Host "Running Disharmony tests ($Runtime, Harmony $version)..."
        if ($Runtime -eq 'Dotnet')
        {
            & $dotnet.Source $testExecutable @effectiveNUnitArguments
            $versionExitCode = $LASTEXITCODE
        }
        elseif ($Runtime -eq 'Clr')
        {
            & $testExecutable @effectiveNUnitArguments
            $versionExitCode = $LASTEXITCODE
        }
        else
        {
            $mono = Resolve-MonoExecutable $MonoExecutable
            $monoArguments = [Collections.Generic.List[string]]::new()

            if ($Profile -or $MonoProfile)
            {
                New-Item -ItemType Directory -Path $resultDirectory -Force | Out-Null
                $profilePath = Join-Path $resultDirectory ("mono-profile-harmony$version-{0:yyyyMMdd-HHmmss}.mlpd" -f [DateTime]::UtcNow)
                $profilePath = $profilePath.Replace('\', '/')

                $profileSpecification = if ($MonoProfile) { $MonoProfile } else { 'log:sample' }
                if ($profileSpecification -notmatch '(^|,)output=')
                {
                    $profileSpecification += ",output=$profilePath"
                    $expectedProfilePath = $profilePath
                    Write-Host "Requesting Mono profile output at $profilePath"
                }
                else
                {
                    Write-Host "Using Mono profiler specification: $profileSpecification"
                }

                $monoArguments.Add("--profile=$profileSpecification")
            }

            $monoArguments.Add($testExecutable)
            foreach ($argument in $effectiveNUnitArguments)
            {
                $monoArguments.Add($argument)
            }

            & $mono @monoArguments
            $versionExitCode = $LASTEXITCODE
        }

        if ($versionExitCode -ne 0)
        {
            $runnerExitCode = $versionExitCode
        }
        if ($versionExitCode -eq 0 -and $expectedProfilePath -and -not (Test-Path -LiteralPath $expectedProfilePath -PathType Leaf))
        {
            throw "Mono completed the tests but did not create $expectedProfilePath. The selected Mono distribution may not include the requested profiler module."
        }
    }
}
finally
{
    $env:DISHARMONY_TEST_HARMONY_MAJOR = $previousExpectedMajor
    Pop-Location
}

exit $runnerExitCode
