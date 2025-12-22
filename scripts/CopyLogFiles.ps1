# 获取脚本当前所在的目录
$currentDir = $PSScriptRoot

# 定义源路径和目标路径
$binPath = Join-Path $currentDir "DeepSightAI"
$logSourcePath = Join-Path $binPath "Log"
$baseDestinationPath = Join-Path $currentDir "LogInfo"

# 获取用户AppData目录下的BoardStatCache路径
$boardStatCachePath = Join-Path $env:LOCALAPPDATA "DeepSightAI"
$boardStatCachePath = Join-Path $boardStatCachePath "BoardStatCache"

# 获取当天的日期，格式为 yyyyMMdd
$today = Get-Date -Format "yyyyMMdd"
# 获取当前的时间戳，用于创建唯一的文件夹
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

# --- 开始执行 ---

# 1. 如果基础目标文件夹 LogInfo 不存在，则创建它
if (-not (Test-Path $baseDestinationPath)) {
    Write-Host "正在创建基础目标文件夹: $baseDestinationPath"
    New-Item -ItemType Directory -Path $baseDestinationPath
}
else {
    Write-Host "基础目标文件夹 LogInfo 已存在。"
}

# 2. 创建本次执行的唯一目标文件夹
$destinationPath = Join-Path $baseDestinationPath $timestamp
Write-Host "正在创建本次执行的目标文件夹: $destinationPath"
New-Item -ItemType Directory -Path $destinationPath

# 3. 拷贝包含当天日期时间戳的日志文件
if (Test-Path $logSourcePath) {
    Write-Host "正在从 $logSourcePath 查找包含 '$today' 的日志文件..."
    $logFiles = Get-ChildItem -Path $logSourcePath -Filter "*.log" | Where-Object { $_.Name -match $today }

    if ($logFiles.Count -gt 0) {
        foreach ($file in $logFiles) {
            Write-Host "正在拷贝 $($file.FullName) 到 $destinationPath"
            Copy-Item -Path $file.FullName -Destination $destinationPath -Force
        }
        Write-Host "$($logFiles.Count) 个日志文件已成功拷贝。"
    }
    else {
        Write-Warning "警告: 未找到包含 '$today' 的日志文件。"
    }

    # 4. 拷贝当天日期的 dataserver_log 文件夹
    Write-Host "正在从 $logSourcePath 查找以 'dataserver_log_$today' 开头的文件夹..."
    $logFolders = Get-ChildItem -Path $logSourcePath -Directory -Filter "dataserver_log_$($today)*"

    if ($logFolders.Count -gt 0) {
        foreach ($folder in $logFolders) {
            Write-Host "正在拷贝文件夹 $($folder.FullName) 到 $destinationPath"
            Copy-Item -Path $folder.FullName -Destination $destinationPath -Recurse -Force
        }
        Write-Host "$($logFolders.Count) 个 dataserver_log 文件夹已成功拷贝。"
    }
    else {
        Write-Warning "警告: 未找到以 'dataserver_log_$today' 开头的文件夹。"
    }
}
else {
    Write-Warning "警告: 未找到日志源文件夹 $logSourcePath"
}

# 5. 拷贝 BoardStatCache 下包含当天日期的 XML 文件
if (Test-Path $boardStatCachePath) {
    Write-Host "正在从 $boardStatCachePath 查找包含 '$today' 的 XML 文件..."
    $xmlFiles = Get-ChildItem -Path $boardStatCachePath -Filter "*.xml" | Where-Object { $_.Name -match $today }

    if ($xmlFiles.Count -gt 0) {
        foreach ($file in $xmlFiles) {
            Write-Host "正在拷贝 $($file.FullName) 到 $destinationPath"
            Copy-Item -Path $file.FullName -Destination $destinationPath -Force
        }
        Write-Host "$($xmlFiles.Count) 个 BoardStatCache XML 文件已成功拷贝。"
    }
    else {
        Write-Warning "警告: 未找到包含 '$today' 的 BoardStatCache XML 文件。"
    }
}
else {
    Write-Warning "警告: 未找到 BoardStatCache 文件夹 $boardStatCachePath"
}

Write-Host "脚本执行完毕。"
# 脚本执行后暂停，以便用户可以看到输出信息
Read-Host "按 Enter 键退出..."
