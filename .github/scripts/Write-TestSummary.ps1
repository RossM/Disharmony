[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Title,

    [string] $ResultDirectory = 'TestResults',

    [string] $SummaryPath = $env:GITHUB_STEP_SUMMARY
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $SummaryPath)
{
    throw 'Specify -SummaryPath when running outside GitHub Actions.'
}

"### $Title`n" | Out-File -LiteralPath $SummaryPath -Encoding utf8 -Append
$resultFiles = @(
    if (Test-Path -LiteralPath $ResultDirectory -PathType Container)
    {
        Get-ChildItem -LiteralPath $ResultDirectory -Recurse -File |
            Where-Object { $_.Extension -eq '.trx' -or $_.Name -like 'test-result-*.xml' }
    }
)
if ($resultFiles.Count -eq 0)
{
    "No test results were produced. Check the job log for build errors or an interrupted test run.`n" |
        Out-File -LiteralPath $SummaryPath -Encoding utf8 -Append
    return
}

$counts = [pscustomobject]@{ Total = 0; Passed = 0; Failed = 0; Skipped = 0 }
foreach ($resultFile in $resultFiles)
{
    [xml] $result = Get-Content -LiteralPath $resultFile.FullName -Raw
    if ($resultFile.Extension -eq '.trx')
    {
        $counters = $result.SelectSingleNode("/*[local-name()='TestRun']/*[local-name()='ResultSummary']/*[local-name()='Counters']")
        $testResults = @($result.SelectNodes("/*[local-name()='TestRun']/*[local-name()='Results']/*[local-name()='UnitTestResult']"))
        $counts.Total += [int] $counters.GetAttribute('total')
        $counts.Passed += [int] $counters.GetAttribute('passed')
        $counts.Failed += [int] $counters.GetAttribute('failed')
        # NUnit's TRX adapter writes ignored tests as NotExecuted but leaves the notExecuted counter at zero.
        $counts.Skipped += @($testResults | Where-Object { $_.GetAttribute('outcome') -eq 'NotExecuted' }).Count
    }
    else
    {
        $testRun = $result.SelectSingleNode('/test-run')
        $counts.Total += [int] $testRun.GetAttribute('total')
        $counts.Passed += [int] $testRun.GetAttribute('passed')
        $counts.Failed += [int] $testRun.GetAttribute('failed')
        $counts.Skipped += [int] $testRun.GetAttribute('skipped')
    }
}

@"
| Total | Passed | Failed | Skipped |
| ---: | ---: | ---: | ---: |
| $($counts.Total) | $($counts.Passed) | $($counts.Failed) | $($counts.Skipped) |

"@ | Out-File -LiteralPath $SummaryPath -Encoding utf8 -Append
$other = $counts.Total - $counts.Passed - $counts.Failed - $counts.Skipped
if ($other -gt 0)
{
    "Tests with other outcomes (such as inconclusive or aborted): $other. Check the test results and job log.`n" |
        Out-File -LiteralPath $SummaryPath -Encoding utf8 -Append
}
