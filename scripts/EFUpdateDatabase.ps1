# Compatibility entry point. Supply a connection explicitly; no implicit
# baseline or destructive schema recreation is performed.
param(
    [Parameter(Mandatory = $true)]
    [string]$Connection,
    [string]$Migration,
    [switch]$Baseline
)

& (Join-Path $PSScriptRoot "DatabaseUpdate.ps1") `
    -Connection $Connection `
    -Migration $Migration `
    -Baseline:$Baseline
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
