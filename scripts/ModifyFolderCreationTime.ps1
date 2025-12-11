# Modify Folder Creation Time Script
# Function: Traverse subfolders and modify creation time

# Get script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Host "Script Path: $scriptPath" -ForegroundColor Green

# Set start time
$startTime = Get-Date "2025-10-01 00:00:00"
Write-Host "Start Time: $startTime" -ForegroundColor Green

# Time interval in seconds
$intervalSeconds = 1

# Counter
$totalProcessed = 0

# Get first level folders
$firstLevelFolders = Get-ChildItem -Path $scriptPath -Directory | Sort-Object Name

Write-Host "`nProcessing folders..." -ForegroundColor Yellow
Write-Host "Found $($firstLevelFolders.Count) first level folders`n" -ForegroundColor Cyan

# Loop through first level folders
foreach ($firstFolder in $firstLevelFolders) {
    Write-Host "Processing: $($firstFolder.Name)" -ForegroundColor Cyan
    
    # Get second level folders
    $secondLevelFolders = Get-ChildItem -Path $firstFolder.FullName -Directory | Sort-Object Name
    
    if ($secondLevelFolders.Count -eq 0) {
        Write-Host "  No second level folders" -ForegroundColor Gray
        continue
    }
    
    Write-Host "  Found $($secondLevelFolders.Count) second level folders" -ForegroundColor Gray
    
    # Loop through second level folders
    foreach ($secondFolder in $secondLevelFolders) {
        # Get third level folders
        $thirdLevelFolders = Get-ChildItem -Path $secondFolder.FullName -Directory -Recurse:$false | Sort-Object Name
        
        if ($thirdLevelFolders.Count -eq 0) {
            Write-Host "    $($secondFolder.Name) - No subfolders" -ForegroundColor DarkGray
            continue
        }
        
        Write-Host "    $($secondFolder.Name) - Found $($thirdLevelFolders.Count) subfolders" -ForegroundColor White
        
        # Sort and modify creation time
        $sortedFolders = $thirdLevelFolders | Sort-Object Name
        
        foreach ($folder in $sortedFolders) {
            try {
                # Calculate new creation time
                $newCreationTime = $startTime.AddSeconds($totalProcessed * $intervalSeconds)
                
                # Modify creation time
                $folder.CreationTime = $newCreationTime
                
                Write-Host "      [OK] $($folder.Name) -> $newCreationTime" -ForegroundColor Green
                
                $totalProcessed++
            }
            catch {
                Write-Host "      [ERROR] $($folder.Name) failed: $_" -ForegroundColor Red
            }
        }
    }
    
    Write-Host ""
}

Write-Host "`nCompleted!" -ForegroundColor Green
Write-Host "Total processed: $totalProcessed folders" -ForegroundColor Green
Write-Host "Time range: $startTime to $($startTime.AddSeconds($totalProcessed * $intervalSeconds))" -ForegroundColor Cyan

# Ask to view results
$response = Read-Host "`nShow modified folder list? (Y/N)"
if ($response -eq 'Y' -or $response -eq 'y') {
    Write-Host "`nModified folder list:" -ForegroundColor Yellow
    
    foreach ($firstFolder in $firstLevelFolders) {
        $secondLevelFolders = Get-ChildItem -Path $firstFolder.FullName -Directory | Sort-Object Name
        
        foreach ($secondFolder in $secondLevelFolders) {
            $thirdLevelFolders = Get-ChildItem -Path $secondFolder.FullName -Directory | Sort-Object Name
            
            if ($thirdLevelFolders.Count -gt 0) {
                Write-Host "`n$($firstFolder.Name)\$($secondFolder.Name):" -ForegroundColor Cyan
                
                foreach ($folder in $thirdLevelFolders) {
                    Write-Host "  $($folder.Name) - Created: $($folder.CreationTime)" -ForegroundColor White
                }
            }
        }
    }
}

# Prevent window from closing
Write-Host "`nPress any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')

