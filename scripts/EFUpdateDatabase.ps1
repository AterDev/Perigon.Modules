# 使用 AdminService 的启动配置更新数据库并执行 EF Core 种子逻辑
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptRoot ".."))
$adminServicePath = Join-Path $repoRoot "src\Services\AdminService"
$adminServiceProjectPath = Join-Path $adminServicePath "AdminService.csproj"
$entityFrameworkProjectPath = Join-Path $repoRoot "src\Definition\EntityFramework\EntityFramework.csproj"

Push-Location $adminServicePath
try {
    dotnet ef database update -c DefaultDbContext --project $entityFrameworkProjectPath --startup-project $adminServiceProjectPath
}
finally { Pop-Location }
