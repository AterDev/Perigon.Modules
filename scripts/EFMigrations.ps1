# Compatibility entry point for existing workflows. The implementation uses
# Perigon.PostgreSQL migrations; no EF Core command is invoked.
param(
    [Parameter(Mandatory = $true)]
    [string]$Name
)

& (Join-Path $PSScriptRoot "Migrations.ps1") -Command Add -Name $Name
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
