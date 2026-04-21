# Class rename helper:
# 1) Rename <Folder>\<OldName>.cs / .Designer.cs / .resx -> <NewName>.* (if exist)
# 2) Replace whole-word OldName -> NewName in all .cs/.csproj/.resx in all project folders
# Usage:
#   .\_rename_class.ps1 -OldName FrHome -NewName FrmHome -Folder DeepSightAI
param(
    [Parameter(Mandatory=$true)][string]$OldName,
    [Parameter(Mandatory=$true)][string]$NewName,
    [Parameter(Mandatory=$true)][string]$Folder
)

$ErrorActionPreference = 'Stop'
$utf8bom = New-Object System.Text.UTF8Encoding $true

# Step 1: rename files
$exts = @('.cs', '.Designer.cs', '.resx')
foreach ($ext in $exts) {
    $oldPath = Join-Path $Folder ($OldName + $ext)
    $newPath = Join-Path $Folder ($NewName + $ext)
    if (Test-Path -LiteralPath $oldPath) {
        if (Test-Path -LiteralPath $newPath) {
            Write-Warning ("Target already exists, skip rename: " + $newPath)
        } else {
            Move-Item -LiteralPath $oldPath -Destination $newPath
            Write-Host ("Renamed: {0} -> {1}" -f $oldPath, $newPath)
        }
    }
}

# Step 2: replace whole-word occurrences in all source files
$projectRoots = @(
    'DeepSightAI',
    'DeepSightCommunication',
    'DeepSightDB',
    'DeepSightDisplay',
    'DeepSightEvent',
    'DeepSightHeatMap',
    'DeepSightModel',
    'DeepSightTool',
    'DeepSightWorkLib',
    'DeepSightWorkLib.Tests'
)
$pattern = "\b" + [regex]::Escape($OldName) + "\b"
$total = 0
$touched = 0

foreach ($root in $projectRoots) {
    if (-not (Test-Path -LiteralPath $root)) { continue }
    $files = Get-ChildItem -Path $root -Recurse -Include *.cs,*.csproj,*.resx -File |
             Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    foreach ($f in $files) {
        $content = [System.IO.File]::ReadAllText($f.FullName, $utf8bom)
        $matches = [regex]::Matches($content, $pattern)
        if ($matches.Count -eq 0) { continue }
        $new = [regex]::Replace($content, $pattern, $NewName)
        [System.IO.File]::WriteAllText($f.FullName, $new, $utf8bom)
        Write-Host ("  {0}: {1}" -f $f.FullName, $matches.Count)
        $total = $total + $matches.Count
        $touched = $touched + 1
    }
}

Write-Host ("Done. {0} files touched, {1} replacements." -f $touched, $total)
