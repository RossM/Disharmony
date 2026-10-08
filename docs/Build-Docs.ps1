[CmdletBinding()]
param(
    [switch] $Serve,
    [ValidateRange(1, 65535)]
    [int] $Port = 8080
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$guideDirectory = Join-Path $PSScriptRoot 'obj/guides'
New-Item -ItemType Directory -Path $guideDirectory -Force | Out-Null

# Keep repository READMEs as the guide sources; adapt their source-code links for HTML.
$guideSources = @{
    'Disharmony/README.md' = 'getting-started.md'
    'Disharmony.Analyzers/README.md' = 'analyzers.md'
    'Disharmony.Tests/README.md' = 'testing.md'
}
foreach ($source in $guideSources.Keys) {
    $content = [IO.File]::ReadAllText((Join-Path $repositoryRoot $source))
    $content = $content.Replace('(Attributes.cs)', '(xref:Disharmony)')
    $content = $content.Replace('(Patch.cs)', '(xref:Disharmony.Patch)')
    $content = $content.Replace('(Patcher.cs)', '(xref:Disharmony.Patcher)')
    $content = $content.Replace('(../Disharmony.Analyzers/README.md)', '(analyzers.md)')
    $content = $content.Replace('(../Disharmony.Tests/README.md)', '(testing.md)')
    [IO.File]::WriteAllText((Join-Path $guideDirectory $guideSources[$source]), $content, [Text.UTF8Encoding]::new($false))
}

Push-Location $repositoryRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Restoring the pinned documentation tool failed.' }

    # Assembly + XML extraction includes C# 14 extension members that project extraction omits.
    dotnet build (Join-Path $repositoryRoot 'Disharmony/Disharmony.csproj') --configuration Release --framework net10.0 --output (Join-Path $PSScriptRoot 'obj/reference')
    if ($LASTEXITCODE -ne 0) { throw 'Building the documentation assembly failed.' }

    dotnet docfx (Join-Path $PSScriptRoot 'docfx.json') --warningsAsErrors
    if ($LASTEXITCODE -ne 0) { throw 'Documentation generation failed. See the DocFX diagnostics above.' }

    # DocFX can succeed while silently omitting extension blocks; guard the affected APIs.
    $referenceChecks = @{
        'Disharmony.Patch.yml' = @(
            'M:Disharmony.Patch.Options(Disharmony.PatchConfig,Disharmony.PatchOptions)',
            'M:Disharmony.Patch.With(Disharmony.PatchConfig,System.Reflection.MethodInfo)',
            'M:Disharmony.Patch.get_Prefix(Disharmony.PatchConfig)'
        )
        'Disharmony.ReflectionExtensions.yml' = @(
            'M:Disharmony.ReflectionExtensions.get_FullName(System.Reflection.MemberInfo)',
            'M:Disharmony.ReflectionExtensions.GetStateMachineImplementation(System.Reflection.MethodBase)'
        )
    }
    foreach ($file in $referenceChecks.Keys) {
        $metadata = [IO.File]::ReadAllText((Join-Path $PSScriptRoot "api/$file"))
        foreach ($member in $referenceChecks[$file]) {
            if (-not $metadata.Contains("commentId: $member")) {
                throw "API extraction omitted $member. Check DocFX extension-member support."
            }
        }
    }

    Write-Host "HTML documentation: $(Join-Path $PSScriptRoot '_site/index.html')"
    if ($Serve) {
        dotnet docfx serve (Join-Path $PSScriptRoot '_site') --hostname localhost --port $Port
        if ($LASTEXITCODE -ne 0) { throw 'The documentation preview server failed.' }
    }
}
finally {
    Pop-Location
}
