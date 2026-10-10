[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$sourcePath = Join-Path $repositoryRoot 'Disharmony.RimWorld/About/README.md'
$outputPath = Join-Path $repositoryRoot 'artifacts/workshop-description.txt'
$content = [IO.File]::ReadAllText($sourcePath)

# Keep the Markdown shared with the HTML player guide as the description source.
$content = $content.Replace('(../README.md)', '(https://rossm.github.io/Disharmony/guides/rimworld.html)')
$content = [regex]::Replace($content, '(?m)^(#{1,3}) ([^\r\n]+)', [Text.RegularExpressions.MatchEvaluator]{
    param($match)
    $level = $match.Groups[1].Length
    "[h$level]$($match.Groups[2].Value)[/h$level]"
})
$content = [regex]::Replace($content, '\*\*([^*\r\n]+)\*\*', '[b]$1[/b]')
$content = [regex]::Replace($content, '\[([^\]\r\n]+)\]\(([^)\r\n]+)\)', [Text.RegularExpressions.MatchEvaluator]{
    param($match)
    $url = $match.Groups[2].Value
    if (-not $url.StartsWith('https://')) {
        throw "Map the Workshop link to an absolute HTTPS URL: $url"
    }
    "[url=$url]$($match.Groups[1].Value)[/url]"
})
$content = [regex]::Replace($content, '(?m)^(?:\d+\. [^\r\n]+\r\n)+', [Text.RegularExpressions.MatchEvaluator]{
    param($match)
    $items = [regex]::Replace($match.Value, '(?m)^\d+\. ', '[*]')
    "[olist]`r`n${items}[/olist]`r`n"
})
if ($content -match '(?m)^#+ |^\d+\. |\*\*|\]\(') {
    throw 'The Workshop description contains Markdown that was not converted.'
}

New-Item -ItemType Directory -Path (Split-Path -Parent $outputPath) -Force | Out-Null
[IO.File]::WriteAllText($outputPath, $content, [Text.UTF8Encoding]::new($false))
Write-Host "Steam Workshop description: $outputPath"
