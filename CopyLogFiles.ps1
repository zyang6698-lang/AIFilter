# 获取脚本当前所在的目录
$currentDir = $PSScriptRoot

# 定义源路径和目标路径
$binPath = Join-Path $currentDir "DeepsightAI"
$logSourcePath = Join-Path $binPath "Log"
$dbSourcePath = Join-Path $binPath "deepsight.db"
$baseDestinationPath = Join-Path $currentDir "LogInfo"

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

# 3. 拷贝 deepsight.db 文件
if (Test-Path $dbSourcePath) {
    Write-Host "正在拷贝 $dbSourcePath 到 $destinationPath"
    Copy-Item -Path $dbSourcePath -Destination $destinationPath -Force
}
else {
    Write-Warning "警告: 未找到数据库文件 $dbSourcePath"
}

# 4. 拷贝当天日期的日志文件
if (Test-Path $logSourcePath) {
    Write-Host "正在从 $logSourcePath 查找以 '$today' 开头的日志文件..."
    $logFiles = Get-ChildItem -Path $logSourcePath -Filter "$($today)*.log"
    
    if ($logFiles.Count -gt 0) {
        foreach ($file in $logFiles) {
            Write-Host "正在拷贝 $($file.FullName) 到 $destinationPath"
            Copy-Item -Path $file.FullName -Destination $destinationPath -Force
        }
        Write-Host "$($logFiles.Count) 个日志文件已成功拷贝。"
    }
    else {
        Write-Warning "警告: 未找到以 '$today' 开头的日志文件。"
    }
}
else {
    Write-Warning "警告: 未找到日志源文件夹 $logSourcePath"
}

Write-Host "脚本执行完毕。"
# 脚本执行后暂停，以便用户可以看到输出信息
Read-Host "按 Enter 键退出..."
