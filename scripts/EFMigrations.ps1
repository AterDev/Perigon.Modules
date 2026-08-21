# 生成 EF Core 迁移
param (
    [Parameter()]
    [string] $Name = $null,
    [Parameter()]
    [string] $DatabaseType = "PostgreSQL"
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptRoot ".."))
$appSettingsPath = Join-Path $repoRoot "src\AppHost\appsettings.Development.json"
$adminServicePath = Join-Path $repoRoot "src\Services\AdminService"
$adminServiceProjectPath = Join-Path $adminServicePath "AdminService.csproj"
$entityFrameworkProjectPath = Join-Path $repoRoot "src\Definition\EntityFramework\EntityFramework.csproj"

$toolManifestPath = @(
    (Join-Path $repoRoot ".config\dotnet-tools.json"),
    (Join-Path $repoRoot "dotnet-tools.json")
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($toolManifestPath) {
    Push-Location $repoRoot
    try { dotnet tool restore } finally { Pop-Location }
}

$isMultiTenant = $true
if (Test-Path $appSettingsPath) {
    try {
        $config = Get-Content $appSettingsPath | ConvertFrom-Json
        if ($null -ne $config.Components.Database) { $DatabaseType = $config.Components.Database }
        if ($null -ne $config.Components.IsMultiTenant) { $isMultiTenant = $config.Components.IsMultiTenant }
    }
    catch {
        Write-Warning "Failed to read $appSettingsPath. Using default database type: $DatabaseType"
    }
}

$env:Components__Database = $DatabaseType
$env:Components__IsMultiTenant = $isMultiTenant

if (-not (Test-Path $adminServiceProjectPath)) { throw "AdminService project not found: $adminServiceProjectPath" }
if (-not (Test-Path $entityFrameworkProjectPath)) { throw "EntityFramework project not found: $entityFrameworkProjectPath" }

Push-Location $adminServicePath
try {
    if ([string]::IsNullOrWhiteSpace($Name)) { $Name = [DateTime]::Now.ToString("yyyyMMdd-HHmmss") }
    dotnet build
    if ($Name -eq "Remove") {
        dotnet ef migrations remove -c DefaultDbContext --no-build --project $entityFrameworkProjectPath --startup-project $adminServiceProjectPath
    }
    else {
        dotnet ef migrations add $Name -c DefaultDbContext --no-build --project $entityFrameworkProjectPath --startup-project $adminServiceProjectPath
    }
}
finally { Pop-Location }
