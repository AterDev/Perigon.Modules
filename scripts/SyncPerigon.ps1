# 同步 Perigon 基础库代码脚本
# 从 Perigon.template 项目同步最新代码到当前项目

# 获取脚本所在目录的父目录（项目根目录）
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath

# 源路径和目标路径
$sourcePath = Join-Path $projectRoot "..\Perigon.template\ApiStandard\src\Perigon"
$targetPath = Join-Path $projectRoot "src\Perigon"

# 检查源路径是否存在
if (-not (Test-Path $sourcePath)) {
    Write-Error "源路径不存在: $sourcePath"
    Write-Host "请确保 Perigon.template 项目位于正确的位置" -ForegroundColor Red
    exit 1
}

Write-Host "开始同步 Perigon 基础库..." -ForegroundColor Cyan
Write-Host "源路径: $sourcePath" -ForegroundColor Gray
Write-Host "目标路径: $targetPath" -ForegroundColor Gray
Write-Host ""

# 使用 robocopy 进行同步
# /MIR - 镜像目录树（复制子目录，包括空目录，删除目标中不存在的文件）
# /XD - 排除目录
# /XF - 排除文件
# /NFL - 不记录文件列表
# /NDL - 不记录目录列表
# /NP - 不显示进度百分比
# /R:3 - 重试次数
# /W:1 - 重试等待时间（秒）

$excludeDirs = @("bin", "obj")
$robocopyArgs = @(
    $sourcePath,
    $targetPath,
    "/MIR",
    "/XD", $excludeDirs,
    "/R:3",
    "/W:1",
    "/NP"
)

# 执行 robocopy
& robocopy @robocopyArgs

$sourcePath = Join-Path $projectRoot "..\Perigon.template\ApiStandard\src\Definition"
$targetPath = Join-Path $projectRoot "src\Definition"
$excludeDirs = @("bin", "obj", "Entity", "AppDbContext")
$excludeFiles = @("GlobalUsings.cs")
$robocopyArgs = @(
    $sourcePath,
    $targetPath,
    "/MIR",
    "/XD", $excludeDirs,
    "/XF", $excludeFiles,
    "/R:3",
    "/W:1",
    "/NP"
)

& robocopy @robocopyArgs

# robocopy 的退出代码含义:
# 0 = 没有复制文件，没有失败，没有不匹配的文件
# 1 = 成功复制了文件
# 2 = 有额外文件或目录
# 4 = 有不匹配的文件或目录
# 8 = 有复制失败的文件
$exitCode = $LASTEXITCODE

if ($exitCode -ge 8) {
    Write-Host ""
    Write-Error "同步过程中出现错误 (退出码: $exitCode)"
    exit 1
}
else {
    Write-Host ""
    Write-Host "✓ Perigon 基础库同步完成！" -ForegroundColor Green
    
    if ($exitCode -eq 0) {
        Write-Host "  没有文件需要更新" -ForegroundColor Yellow
    }
    elseif ($exitCode -eq 1) {
        Write-Host "  文件已成功更新" -ForegroundColor Green
    }
    elseif ($exitCode -ge 2) {
        Write-Host "  已同步，可能有额外或不匹配的文件" -ForegroundColor Yellow
    }
}

# 正常退出
exit 0
