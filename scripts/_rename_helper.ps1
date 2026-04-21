# Batch rename helper script
# Usage: .\_rename_helper.ps1 -OldName <name> -NewName <name> -Files <paths> [-WholeWord]
# Reads/writes files as UTF-8 with BOM to preserve Designer.cs encoding.
param(
    [Parameter(Mandatory=$true)][string]$OldName,
    [Parameter(Mandatory=$true)][string]$NewName,
    [Parameter(Mandatory=$true)][string[]]$Files,
    [switch]$WholeWord
)

$pattern = if ($WholeWord) { "\b" + [regex]::Escape($OldName) + "\b" } else { [regex]::Escape($OldName) }
$utf8bom = New-Object System.Text.UTF8Encoding $true

# Flatten file list: split any comma-joined strings into individual paths
$fileList = @()
foreach ($item in $Files) {
    foreach ($sub in ($item -split ',')) {
        $trim = $sub.Trim()
        if ($trim) { $fileList += $trim }
    }
}

$total = 0
foreach ($f in $fileList) {
    if (-not (Test-Path -LiteralPath $f)) {
        Write-Warning ("File not found: " + $f)
        continue
    }
    $full = (Resolve-Path -LiteralPath $f).Path
    $content = [System.IO.File]::ReadAllText($full, $utf8bom)
    $matchCount = ([regex]::Matches($content, $pattern)).Count
    if ($matchCount -eq 0) { continue }
    $new = [regex]::Replace($content, $pattern, $NewName)
    [System.IO.File]::WriteAllText($full, $new, $utf8bom)
    Write-Host ("{0}: {1} replacements" -f $f, $matchCount)
    $total = $total + $matchCount
}
Write-Host ("Total: {0} replacements" -f $total)
