<#
.SYNOPSIS
    Ingests individual WinUI samples from the samples/ folder into the combined
    SamplesBrowser application.

.DESCRIPTION
    This script:
      1. Scans samples/ for all sample folders (those containing Sample.xaml).
      2. Copies Sample.xaml and Sample.xaml.cs into the browser's Pages/ directory,
         updating namespaces and x:Class attributes.
      3. Generates Assets/toc.json with the full group/component/sample hierarchy.
      4. Generates SampleRegistry.cs that maps routes to UserControl factories.
      5. Updates SamplesBrowser.csproj to include all copied page files.

.NOTES
    Run from the scripts/ directory:
        .\ingest-samples.ps1

    Or from the repository root:
        .\scripts\ingest-samples.ps1
#>

[CmdletBinding()]
param(
    [string]$SamplesRoot  = $null,
    [string]$BrowserRoot  = $null
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Resolve paths ────────────────────────────────────────────────────────────

$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot    = Split-Path -Parent $scriptDir

if (-not $SamplesRoot) { $SamplesRoot = Join-Path $repoRoot "samples" }
if (-not $BrowserRoot)  { $BrowserRoot  = Join-Path $repoRoot "browser\SamplesBrowser" }

$pagesDir    = Join-Path $BrowserRoot "Pages"
$assetsDir   = Join-Path $BrowserRoot "Assets"
$csprojPath  = Join-Path $BrowserRoot "SamplesBrowser.csproj"
$tocPath     = Join-Path $assetsDir   "toc.json"
$registryPath = Join-Path $BrowserRoot "SampleRegistry.cs"

Write-Host ""
Write-Host "=== ingest-samples.ps1 ===" -ForegroundColor Cyan
Write-Host "  Samples root : $SamplesRoot"
Write-Host "  Browser root : $BrowserRoot"
Write-Host ""

# ── Helper: convert a hyphenated folder segment to PascalCase ────────────────

function ConvertTo-PascalCase([string]$segment) {
    ($segment -split '-') | ForEach-Object {
        if ($_.Length -gt 0) { $_.Substring(0,1).ToUpper() + $_.Substring(1) }
    } | Join-String -Separator ""
}

# ── Helper: convert a folder-relative route to a .NET namespace suffix ───────

function Get-Namespace([string]$route) {
    # route: "charts/category-chart/overview"
    ($route -split '/') | ForEach-Object { ConvertTo-PascalCase $_ } | Join-String -Separator "."
}

# ── Clean previously ingested sample pages ───────────────────────────────────

Write-Host "Cleaning previously ingested pages ..." -ForegroundColor Yellow

# Remove only the dynamically-generated sub-folders (not HomePage / SampleHostPage)
$staticPages = @("HomePage.xaml", "HomePage.xaml.cs", "SampleHostPage.xaml", "SampleHostPage.xaml.cs")

Get-ChildItem -Path $pagesDir -Recurse -Directory | Sort-Object FullName -Descending | ForEach-Object {
    Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "  Done." -ForegroundColor Green

# ── Discover samples ─────────────────────────────────────────────────────────

Write-Host ""
Write-Host "Scanning for samples in $SamplesRoot ..." -ForegroundColor Yellow

# A sample folder is any directory that contains both Sample.xaml and Sample.xaml.cs
$sampleFiles = Get-ChildItem -Path $SamplesRoot -Recurse -Filter "Sample.xaml" |
               Where-Object { Test-Path (Join-Path $_.DirectoryName "Sample.xaml.cs") }

if ($sampleFiles.Count -eq 0) {
    Write-Warning "No samples found. Make sure each sample folder contains Sample.xaml and Sample.xaml.cs."
    exit 0
}

Write-Host "  Found $($sampleFiles.Count) sample(s)." -ForegroundColor Green

# ── Build internal sample list ───────────────────────────────────────────────

$sampleList = [System.Collections.Generic.List[PSCustomObject]]::new()

foreach ($xamlFile in $sampleFiles) {
    $sampleDir = $xamlFile.DirectoryName

    # Compute route relative to samples root: "charts/category-chart/overview"
    $route = ($sampleDir -replace [regex]::Escape($SamplesRoot), "").TrimStart('\', '/')
    $route = $route -replace '\\', '/'

    # Extract path segments
    $segments = $route -split '/'
    if ($segments.Count -lt 3) {
        Write-Warning "Skipping '$route' – expected at least 3 path segments (group/component/sample)."
        continue
    }

    $group     = $segments[0]                         # e.g. "charts"
    $component = $segments[1]                         # e.g. "category-chart"
    $sampleSeg = $segments[-1]                        # e.g. "overview"

    # Human-readable names (capitalise first letter of each hyphenated word)
    $groupName     = ($group     -split '-' | ForEach-Object { if ($_.Length -gt 0) { $_.Substring(0,1).ToUpper() + $_.Substring(1) } }) -join ' '
    $componentName = ($component -split '-' | ForEach-Object { if ($_.Length -gt 0) { $_.Substring(0,1).ToUpper() + $_.Substring(1) } }) -join ' '
    $sampleName    = ($sampleSeg -split '-' | ForEach-Object { if ($_.Length -gt 0) { $_.Substring(0,1).ToUpper() + $_.Substring(1) } }) -join ' '

    # .NET namespace suffix inside SamplesBrowser.Pages
    $nsSuffix = Get-Namespace $route    # e.g. "Charts.CategoryChart.Overview"
    $fullNs   = "SamplesBrowser.Pages.$nsSuffix"

    $sampleList.Add([PSCustomObject]@{
        Route         = $route
        Group         = $group
        GroupName     = $groupName
        Component     = $component
        ComponentName = $componentName
        SampleSeg     = $sampleSeg
        SampleName    = $sampleName
        Namespace     = $fullNs
        SrcDir        = $sampleDir
        XamlFile      = $xamlFile.FullName
        CsFile        = Join-Path $sampleDir "Sample.xaml.cs"
    })
}

Write-Host "  Processed $($sampleList.Count) sample(s)." -ForegroundColor Green

# ── Copy sample files into browser/Pages/ ────────────────────────────────────

Write-Host ""
Write-Host "Copying sample files ..." -ForegroundColor Yellow

$copiedItems = [System.Collections.Generic.List[string]]::new()

foreach ($s in $sampleList) {

    $destDir = Join-Path $pagesDir ($s.Route -replace '/', '\')
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null

    # ── Sample.xaml ──────────────────────────────────────────────────────────
    $xamlContent = Get-Content $s.XamlFile -Raw

    # Replace x:Class="WinUIApp.Sample" with the browser namespace
    $xamlContent = $xamlContent -replace 'x:Class="[^"]*"', "x:Class=`"$($s.Namespace).Sample`""

    # Remove any standalone @page directives (not applicable to XAML, but future-proof)
    $destXaml = Join-Path $destDir "Sample.xaml"
    Set-Content -Path $destXaml -Value $xamlContent -Encoding UTF8
    $copiedItems.Add($destXaml)

    # ── Sample.xaml.cs ───────────────────────────────────────────────────────
    $csContent = Get-Content $s.CsFile -Raw

    # Replace namespace declaration
    $csContent = $csContent -replace 'namespace\s+\S+\s*;', "namespace $($s.Namespace);"
    $csContent = $csContent -replace 'namespace\s+\S+\s*\{', "namespace $($s.Namespace) {"

    $destCs = Join-Path $destDir "Sample.xaml.cs"
    Set-Content -Path $destCs -Value $csContent -Encoding UTF8
    $copiedItems.Add($destCs)

    Write-Host "  + $($s.Route)" -ForegroundColor Gray
}

Write-Host "  Copied $($sampleList.Count) sample(s)." -ForegroundColor Green

# ── Generate toc.json ─────────────────────────────────────────────────────────

Write-Host ""
Write-Host "Generating toc.json ..." -ForegroundColor Yellow

# Build a hierarchy: groups → components → samples
$groups = [ordered]@{}

foreach ($s in $sampleList) {
    if (-not $groups.Contains($s.Group)) {
        $groups[$s.Group] = [PSCustomObject]@{
            name       = $s.GroupName
            components = [ordered]@{}
        }
    }

    $g = $groups[$s.Group]
    if (-not $g.components.Contains($s.Component)) {
        $g.components[$s.Component] = [PSCustomObject]@{
            name    = $s.ComponentName
            folder  = $s.Component
            samples = [System.Collections.Generic.List[PSCustomObject]]::new()
        }
    }

    $g.components[$s.Component].samples.Add([PSCustomObject]@{
        name     = $s.SampleName
        route    = $s.Route
        showLink = $true
    })
}

# Convert to a serialisable structure, forcing arrays with @() so that
# ConvertTo-Json never collapses a single-element collection to an object.
$tocGroups = [System.Collections.Generic.List[PSCustomObject]]::new()

foreach ($g in $groups.Values) {
    $compList = [System.Collections.Generic.List[PSCustomObject]]::new()
    foreach ($c in $g.components.Values) {
        $compList.Add([PSCustomObject]@{
            name    = $c.name
            folder  = $c.folder
            samples = @($c.samples.ToArray())
        })
    }
    $tocGroups.Add([PSCustomObject]@{
        name       = $g.name
        components = @($compList.ToArray())
    })
}

$toc = [PSCustomObject]@{ groups = @($tocGroups.ToArray()) }
$tocJson = $toc | ConvertTo-Json -Depth 10
Set-Content -Path $tocPath -Value $tocJson -Encoding UTF8

Write-Host "  Written to $tocPath" -ForegroundColor Green

# ── Generate SampleRegistry.cs ───────────────────────────────────────────────

Write-Host ""
Write-Host "Generating SampleRegistry.cs ..." -ForegroundColor Yellow

$entries = foreach ($s in $sampleList) {
    "        { `"$($s.Route)`", () => new Pages.$($s.Namespace.Replace('SamplesBrowser.Pages.', '')).Sample() },"
}

$registryContent = @"
// AUTO-GENERATED by scripts/ingest-samples.ps1 — DO NOT EDIT MANUALLY.
// Run the ingest script to regenerate this file after adding or removing samples.

using System;
using System.Collections.Generic;

namespace SamplesBrowser;

public static class SampleRegistry
{
    /// <summary>
    /// Maps a sample route (e.g. "charts/category-chart/overview") to a factory
    /// that creates the corresponding sample UserControl.
    /// Populated by scripts/ingest-samples.ps1.
    /// </summary>
    public static readonly Dictionary<string, Func<object>> Samples = new()
    {
$($entries -join "`n")
    };
}
"@

Set-Content -Path $registryPath -Value $registryContent -Encoding UTF8

Write-Host "  Written to $registryPath" -ForegroundColor Green

# ── Update SamplesBrowser.csproj ─────────────────────────────────────────────

Write-Host ""
Write-Host "Updating SamplesBrowser.csproj ..." -ForegroundColor Yellow

[xml]$csproj = Get-Content $csprojPath

# Remove any previously generated <ItemGroup> blocks tagged with our label
$oldGroups = $csproj.Project.ItemGroup | Where-Object {
    $_.GetAttribute("Label") -eq "IngestedSamples"
}
foreach ($og in @($oldGroups)) {
    $csproj.Project.RemoveChild($og) | Out-Null
}

if ($sampleList.Count -gt 0) {
    # Add a new <ItemGroup Label="IngestedSamples"> with Page and Compile items
    $itemGroup = $csproj.CreateElement("ItemGroup")
    $itemGroup.SetAttribute("Label", "IngestedSamples")

    foreach ($s in $sampleList) {
        $relXaml = "Pages\" + ($s.Route -replace '/', '\') + "\Sample.xaml"
        $relCs   = "Pages\" + ($s.Route -replace '/', '\') + "\Sample.xaml.cs"

        # <Page Include="Pages\...\Sample.xaml" />
        $pageEl = $csproj.CreateElement("Page")
        $pageEl.SetAttribute("Include", $relXaml)
        $subType = $csproj.CreateElement("SubType")
        $subType.InnerText = "Designer"
        $generator = $csproj.CreateElement("Generator")
        $generator.InnerText = "MSBuild:Compile"
        $pageEl.AppendChild($subType)  | Out-Null
        $pageEl.AppendChild($generator) | Out-Null
        $itemGroup.AppendChild($pageEl) | Out-Null

        # <Compile Include="Pages\...\Sample.xaml.cs" DependentUpon="Sample.xaml" />
        $compileEl = $csproj.CreateElement("Compile")
        $compileEl.SetAttribute("Include", $relCs)
        $depOn = $csproj.CreateElement("DependentUpon")
        $depOn.InnerText = "Sample.xaml"
        $compileEl.AppendChild($depOn) | Out-Null
        $itemGroup.AppendChild($compileEl) | Out-Null
    }

    $csproj.Project.AppendChild($itemGroup) | Out-Null
}

$csproj.Save($csprojPath)

Write-Host "  Updated $csprojPath" -ForegroundColor Green

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan
Write-Host "  $($sampleList.Count) sample(s) ingested into the browser."
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Open browser\SamplesBrowser\SamplesBrowser.csproj in Visual Studio 2022"
Write-Host "  2. Build (Ctrl+Shift+B) and run (F5)"
Write-Host ""
