# Check whether any of the given class names appear inside C# string literals
# or resx string values. These are places where rename would be unsafe.
param(
    [Parameter(Mandatory=$true)][string[]]$Names
)

$roots = @('DeepSightAI','DeepSightCommunication','DeepSightDB','DeepSightDisplay',
           'DeepSightEvent','DeepSightHeatMap','DeepSightModel','DeepSightTool',
           'DeepSightWorkLib','DeepSightWorkLib.Tests')

$found = $false
foreach ($name in $Names) {
    $pattern = '"' + [regex]::Escape($name) + '"'
    foreach ($root in $roots) {
        if (-not (Test-Path -LiteralPath $root)) { continue }
        $files = Get-ChildItem -Path $root -Recurse -Include *.cs,*.resx,*.csproj -File |
                 Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
        foreach ($f in $files) {
            $hits = Select-String -LiteralPath $f.FullName -Pattern $pattern
            foreach ($h in $hits) {
                $found = $true
                Write-Host ("[{0}] {1}:{2} => {3}" -f $name, $f.FullName, $h.LineNumber, $h.Line.Trim())
            }
        }
    }
}
if (-not $found) { Write-Host "No string-literal occurrences found." }
