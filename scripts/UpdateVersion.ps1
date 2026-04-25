<#
.SYNOPSIS
    从 Git Tag 自动获取版本号并更新所有项目文件。支持自动递增并打 tag。
.DESCRIPTION
    版本号格式: Major.Minor.Patch.Build (例: 1.2.3.0)  Git Tag 格式: v1.2.3

    使用方式:
      .\scripts\UpdateVersion.ps1 -Bump Major   # 1.1.0 -> 2.0.0 (重大变更)
      .\scripts\UpdateVersion.ps1 -Bump Minor   # 1.1.0 -> 1.2.0 (新功能)
      .\scripts\UpdateVersion.ps1 -Bump Patch   # 1.1.0 -> 1.1.1 (Bug修复)
      .\scripts\UpdateVersion.ps1               # 从当前 git tag 读取
      .\scripts\UpdateVersion.ps1 -Version "2.0.0.0"  # 手动指定

    Build 位 = tag 之后的 commit 数 (自动计算，无需手动管理)
#>
param(
    [string]$Bump = "",
    [string]$Version = "",
    [string]$TagPrefix = "v",
    [switch]$NoTag
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Split-Path -Parent $scriptDir

function Get-CurrentVersionFromTag {
    $tag = $null
    try { $tag = git describe --tags --match "$TagPrefix*" --abbrev=0 2>$null } catch {}
    if ($LASTEXITCODE -ne 0 -or -not $tag) { return @{ Tag = $null; Major = 0; Minor = 0; Patch = 0 } }
    $ver = ($tag -replace "^$TagPrefix", "")
    $p = $ver.Split(".")
    return @{
        Tag   = $tag
        Major = [int]$p[0]
        Minor = if ($p.Length -ge 2) { [int]$p[1] } else { 0 }
        Patch = if ($p.Length -ge 3) { [int]$p[2] } else { 0 }
    }
}

# === Determine version ===
$commitCount = "0"
$gitDescribe = $null

if ($Bump -ne "") {
    $cur = Get-CurrentVersionFromTag
    $gitDescribe = $cur.Tag
    switch ($Bump) {
        "Major" { $cur.Major++; $cur.Minor = 0; $cur.Patch = 0 }
        "Minor" { $cur.Minor++; $cur.Patch = 0 }
        "Patch" { $cur.Patch++ }
    }
    $Version = "$($cur.Major).$($cur.Minor).$($cur.Patch)"
    $newTag = "$TagPrefix$Version"
    if (-not $NoTag) {
        Write-Host "  Tag: $newTag" -ForegroundColor Yellow
        git tag $newTag -m "Release $Version"
        if ($LASTEXITCODE -ne 0) { Write-Error "git tag failed: $newTag may exist"; exit 1 }
        $gitDescribe = $newTag
    }
}
elseif ($Version -eq "") {
    try { $gitDescribe = git describe --tags --match "$TagPrefix*" --abbrev=0 2>$null } catch {}
    if ($LASTEXITCODE -ne 0 -or -not $gitDescribe) {
        Write-Warning "No git tag found, using 0.0.0.0"
        $Version = "0.0.0.0"
    } else {
        $Version = ($gitDescribe -replace "^$TagPrefix", "")
    }
    if ($LASTEXITCODE -eq 0 -and $gitDescribe) {
        $commitCount = git rev-list "$gitDescribe..HEAD" --count 2>$null
        if ($LASTEXITCODE -ne 0 -or -not $commitCount) { $commitCount = "0" }
    }
}

# === Parse 4-part version ===
$p = $Version.Split(".")
$major = if ($p.Length -ge 1) { $p[0] } else { "0" }
$minor = if ($p.Length -ge 2) { $p[1] } else { "0" }
$patch = if ($p.Length -ge 3) { $p[2] } else { "0" }
$build = if ($p.Length -ge 4) { $p[3] } else { $commitCount }
$ver4 = "$major.$minor.$patch.$build"
$ver3 = "$major.$minor.$patch"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Version: $ver4" -ForegroundColor Green
if ($gitDescribe) { Write-Host "  Tag:     $gitDescribe" -ForegroundColor DarkGray }
Write-Host "  Build:   $build (commits since tag)" -ForegroundColor DarkGray
Write-Host "========================================" -ForegroundColor Cyan

# === Update AssemblyInfo.cs ===
$asmFiles = Get-ChildItem -Path $repoRoot -Recurse -Filter "AssemblyInfo.cs" |
    Where-Object { $_.FullName -notmatch "\\(obj|bin|packages)\\" }

$rxAV  = [regex]::new('(?m)^(\[assembly:\s*AssemblyVersion\(")[0-9.]+("\)\])')
$rxFV  = [regex]::new('(?m)^(\[assembly:\s*AssemblyFileVersion\(")[0-9.]+("\)\])')
$rxIV  = [regex]::new('(?m)^(\[assembly:\s*AssemblyInformationalVersion\(")[0-9.]+("\)\])')
$repl  = '${1}' + $ver4 + '${2}'

foreach ($f in $asmFiles) {
    $text = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
    $orig = $text
    $text = $rxAV.Replace($text, $repl)
    $text = $rxFV.Replace($text, $repl)
    $text = $rxIV.Replace($text, $repl)
    if ($text -ne $orig) {
        [System.IO.File]::WriteAllText($f.FullName, $text, [System.Text.UTF8Encoding]::new($true))
        $rel = $f.FullName.Substring($repoRoot.Length + 1)
        Write-Host "  [OK] $rel -> $ver4" -ForegroundColor Green
    }
}

# === Update installer .iss ===
$issPath = Join-Path $repoRoot "installer\DeepSightAI.iss"
if (Test-Path $issPath) {
    $rxISS = [regex]::new('(#define\s+MyAppVersion\s+")[0-9.]+(")')
    $text = [System.IO.File]::ReadAllText($issPath, [System.Text.Encoding]::UTF8)
    $orig = $text
    $text = $rxISS.Replace($text, $repl)
    if ($text -ne $orig) {
        [System.IO.File]::WriteAllText($issPath, $text, [System.Text.UTF8Encoding]::new($true))
        Write-Host "  [OK] installer\DeepSightAI.iss -> $ver4" -ForegroundColor Green
    }
}

# === CI output ===
if ($env:GITHUB_OUTPUT) {
    "version=$ver4" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
}

Write-Host ""
Write-Host "Done: $ver4" -ForegroundColor Cyan
if ($Bump -ne "" -and -not $NoTag) {
    $pushCmd = "git push origin $TagPrefix$ver3"
    Write-Host "Push tag:  $pushCmd" -ForegroundColor Yellow
}

