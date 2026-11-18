# Perigon.PostgreSQL migration authoring and inspection.
param(
    [Parameter(Position = 0)]
    [ValidateSet("Add", "List", "Script")]
    [string]$Command = "List",
    [Parameter(Position = 1)]
    [string]$Name,
    [string]$From = "0",
    [string]$To,
    [switch]$Idempotent,
    [string]$Output
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptRoot ".."))
$entityProjectPath = Join-Path $repoRoot "src\Definition\EntityFramework\EntityFramework.csproj"
$migrationOutputPath = Join-Path $repoRoot "src\Definition\EntityFramework\Migrations"

if (-not (Test-Path $entityProjectPath)) {
    throw "Entity project not found: $entityProjectPath"
}

function Invoke-Perigon {
    param([string[]]$Arguments)

    & dotnet perigon @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Perigon PostgreSQL migration command failed with exit code $LASTEXITCODE."
    }
}

switch ($Command) {
    "Add" {
        if ([string]::IsNullOrWhiteSpace($Name)) {
            throw "-Name is required when -Command Add is used."
        }

        Invoke-Perigon @(
            "migrations", "add", $Name,
            "--project", $entityProjectPath,
            "--context", "DefaultDbContext",
            "--output", $migrationOutputPath
        )
    }
    "List" {
        Invoke-Perigon @(
            "migrations", "list",
            "--project", $entityProjectPath,
            "--context", "DefaultDbContext"
        )
    }
    "Script" {
        $arguments = @(
            "migrations", "script",
            "--project", $entityProjectPath,
            "--context", "DefaultDbContext",
            "--from", $From
        )
        if (-not [string]::IsNullOrWhiteSpace($To)) { $arguments += @("--to", $To) }
        if ($Idempotent) { $arguments += "--idempotent" }
        if (-not [string]::IsNullOrWhiteSpace($Output)) { $arguments += @("--output", $Output) }
        Invoke-Perigon $arguments
    }
}
