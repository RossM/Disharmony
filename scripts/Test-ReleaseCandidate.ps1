#Requires -Version 7.4

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [string] $MonoExecutable
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'Use a new test output directory.' }
New-Item -ItemType Directory -Path $output | Out-Null
$pwsh = Join-Path $PSHOME 'pwsh.exe'

Start-Transcript -Path (Join-Path $output 'tests.log') | Out-Null
Push-Location $repositoryRoot
try
{
    foreach ($configuration in @('Debug', 'Release'))
    {
        $results = Join-Path $output $configuration
        New-Item -ItemType Directory -Path $results | Out-Null
        foreach ($runtime in @('Clr', 'Mono', 'Dotnet'))
        {
            $arguments = @(
                '-NoProfile', '-File', (Join-Path $repositoryRoot 'Disharmony.Tests/Run-DisharmonyTests.ps1'),
                '-Runtime', $runtime, '-Configuration', $configuration
            )
            if ($runtime -eq 'Mono' -and $MonoExecutable) { $arguments += @('-MonoExecutable', $MonoExecutable) }
            # The existing runner exits with NUnit's status; use a child process to retain it reliably.
            $started = [DateTime]::UtcNow
            & $pwsh @arguments
            $testExitCode = $LASTEXITCODE
            $harmonyVersions = if ($runtime -eq 'Dotnet') { @('2', '3') } else { @('2') }
            foreach ($harmony in $harmonyVersions)
            {
                $resultPath = Join-Path $repositoryRoot "TestResults/Disharmony/test-result-$($runtime.ToLowerInvariant())-harmony$harmony.xml"
                # Retain newly written results even when tests fail, without copying stale results from an earlier run.
                if ((Test-Path -LiteralPath $resultPath -PathType Leaf) -and (Get-Item -LiteralPath $resultPath).LastWriteTimeUtc -ge $started)
                {
                    Copy-Item -LiteralPath $resultPath -Destination $results
                }
                elseif ($testExitCode -eq 0) { throw "Missing new NUnit results: $resultPath" }
            }
            if ($testExitCode -ne 0) { throw "$configuration/$runtime failed with exit code $testExitCode. See $output." }
        }

        dotnet test (Join-Path $repositoryRoot 'Disharmony.Analyzers.Tests/Disharmony.Analyzers.Tests.csproj') -c $configuration --logger trx --results-directory (Join-Path $results 'analyzers')
        if ($LASTEXITCODE -ne 0) { throw "$configuration analyzer tests failed with exit code $LASTEXITCODE. See $output." }
    }
    Write-Host "PASS: Debug and Release runtime/Harmony and analyzer suites. Results: $output"
}
finally
{
    Pop-Location
    Stop-Transcript | Out-Null
}
