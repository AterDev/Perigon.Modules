# Apply or explicitly baseline the Perigon.PostgreSQL schema.
param(
    [Parameter(Mandatory = $true)]
    [string]$Connection,
    [string]$Migration,
    [switch]$Baseline
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptRoot ".."))
$entityProjectPath = Join-Path $repoRoot "src\Definition\EntityFramework\EntityFramework.csproj"

if (-not (Test-Path $entityProjectPath)) {
    throw "Entity project not found: $entityProjectPath"
}

function Invoke-Perigon {
    param([string[]]$Arguments)

    & dotnet perigon @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Perigon PostgreSQL database command failed with exit code $LASTEXITCODE."
    }
}

if ($Baseline) {
    if ([string]::IsNullOrWhiteSpace($Migration)) {
        throw "-Migration is required for an explicit baseline. No automatic baseline is performed."
    }

    Invoke-Perigon @(
        "database", "baseline",
        "--project", $entityProjectPath,
        "--context", "DefaultDbContext",
        "--connection", $Connection,
        "--migration", $Migration
    )
    exit 0
}

$arguments = @(
    "database", "update",
    "--project", $entityProjectPath,
    "--context", "DefaultDbContext",
    "--connection", $Connection
)
if (-not [string]::IsNullOrWhiteSpace($Migration)) { $arguments += @("--target", $Migration) }
Invoke-Perigon $arguments
