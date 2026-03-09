[CmdletBinding(SupportsShouldProcess)]
param ()

$PSDefaultParameterValues['*:Encoding'] = 'utf8'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-LatestModulePackage {
	param (
		[Parameter(Mandatory)]
		[string]$PackageRoot,

		[Parameter(Mandatory)]
		[string]$ModuleName,

		[Parameter(Mandatory)]
		[datetime]$PackStartTime
	)

	$candidates = Get-ChildItem -Path $PackageRoot -Filter *.zip -File |
		Where-Object {
			$_.Name -like "$ModuleName*.zip" -and $_.LastWriteTimeUtc -ge $PackStartTime.ToUniversalTime().AddSeconds(-1)
		} |
		Sort-Object LastWriteTimeUtc -Descending

	return $candidates | Select-Object -First 1
}

function Get-MetadataFromZip {
	param (
		[Parameter(Mandatory)]
		[string]$ZipPath
	)

	$archive = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)
	try {
		$entry = $archive.Entries |
			Where-Object { $_.FullName -match '(^|/)metadata\.json$' } |
			Select-Object -First 1

		if (-not $entry) {
			throw "压缩包 '$ZipPath' 中未找到 metadata.json。"
		}

		$reader = [System.IO.StreamReader]::new($entry.Open(), [System.Text.Encoding]::UTF8)
		try {
			$metadataContent = $reader.ReadToEnd()
		}
		finally {
			$reader.Dispose()
		}

		return $metadataContent | ConvertFrom-Json
	}
	finally {
		$archive.Dispose()
	}
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptRoot '..')
$modulesRoot = Join-Path $projectRoot 'src\Modules'
$packageRoot = Join-Path $projectRoot 'package_modules'
$modulesJsonPath = Join-Path $scriptRoot 'modules.json'
$serviceName = 'AdminService'

if (-not (Test-Path $modulesRoot)) {
	throw "模块目录不存在：$modulesRoot"
}

if (-not (Test-Path $packageRoot)) {
	New-Item -Path $packageRoot -ItemType Directory | Out-Null
}

$moduleDirectories = Get-ChildItem -Path $modulesRoot -Directory |
	Where-Object { $_.Name.EndsWith('Mod') } |
	Sort-Object Name

if ($moduleDirectories.Count -eq 0) {
	'[]' | Set-Content -Path $modulesJsonPath -Encoding UTF8
	Write-Warning '未找到任何以 Mod 结尾的模块，已输出空的 modules.json。'
	return
}

$existingPackages = Get-ChildItem -Path $packageRoot -Filter *.zip -File
foreach ($package in $existingPackages) {
	if ($PSCmdlet.ShouldProcess($package.FullName, '删除旧模块压缩包')) {
		Remove-Item -Path $package.FullName -Force
	}
}

$location = Get-Location
$allMetadata = [System.Collections.Generic.List[object]]::new()

try {
	Set-Location $projectRoot

	foreach ($moduleDirectory in $moduleDirectories) {
		$moduleName = $moduleDirectory.Name
		$packStartTime = [datetime]::UtcNow
		Write-Host "开始打包模块：$moduleName (Service: $serviceName)" -ForegroundColor Cyan

		if ($PSCmdlet.ShouldProcess($moduleName, "执行 perigon pack $moduleName $serviceName")) {
			& perigon pack $moduleName $serviceName
			if ($LASTEXITCODE -ne 0) {
				throw "perigon pack $moduleName $serviceName 执行失败，退出码：$LASTEXITCODE"
			}

			$packageFile = Get-LatestModulePackage -PackageRoot $packageRoot -ModuleName $moduleName -PackStartTime $packStartTime
			if (-not $packageFile) {
				throw "未在目录 '$packageRoot' 中找到模块 '$moduleName' 的打包结果。"
			}

			$metadata = Get-MetadataFromZip -ZipPath $packageFile.FullName
			$allMetadata.Add($metadata)
			Write-Host "  ✓ 已生成：$($packageFile.Name)" -ForegroundColor Green
		}
	}
}
finally {
	Set-Location $location
}

$sortedMetadata = $allMetadata |
	Sort-Object ModuleName

if ($PSCmdlet.ShouldProcess($modulesJsonPath, '写入模块元数据汇总')) {
	$sortedMetadata | ConvertTo-Json -Depth 10 | Set-Content -Path $modulesJsonPath -Encoding UTF8
}

Write-Host "完成，共处理 $($moduleDirectories.Count) 个模块。" -ForegroundColor Green
Write-Host "压缩包输出目录：$packageRoot" -ForegroundColor Green
Write-Host "元数据汇总文件：$modulesJsonPath" -ForegroundColor Green
