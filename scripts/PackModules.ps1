[CmdletBinding(SupportsShouldProcess)]
param (
	[Parameter()]
	[string[]]$Modules,

	[Parameter()]
	[ValidateSet('none', 'major', 'minor', 'patch')]
	[string]$Bump = 'none'
)

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

	$candidates = Get-ChildItem -Path $PackageRoot -Filter "$ModuleName.zip" -File |
		Where-Object {
			$_.LastWriteTimeUtc -ge $PackStartTime.ToUniversalTime().AddSeconds(-1)
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

function Read-ModuleCatalog {
	param (
		[Parameter(Mandatory)]
		[string]$CatalogPath
	)

	if (-not (Test-Path -LiteralPath $CatalogPath -PathType Leaf)) {
		return @()
	}

	$content = Get-Content -LiteralPath $CatalogPath -Raw
	if ([string]::IsNullOrWhiteSpace($content)) {
		return @()
	}

	$catalog = $content | ConvertFrom-Json
	if ($null -eq $catalog) {
		return @()
	}

	return @($catalog)
}

function Find-ModuleCatalogEntry {
	param (
		[Parameter()]
		[object[]]$Catalog,

		[Parameter(Mandatory)]
		[string]$ModuleName
	)

	return $Catalog |
		Where-Object { $_.ModuleName -eq $ModuleName } |
		Select-Object -First 1
}

function Get-ModuleVersion {
	param (
		[Parameter()]
		[object]$CatalogEntry,

		[Parameter(Mandatory)]
		[ValidateSet('none', 'major', 'minor', 'patch')]
		[string]$Bump
	)

	$defaultVersion = '1.0.0'
	if ($null -eq $CatalogEntry) {
		# A module without a catalog entry is a first publication, not an update.
		return $defaultVersion
	}

	if (-not $CatalogEntry.PSObject.Properties['Version']) {
		throw "modules.json 中模块 '$($CatalogEntry.ModuleName)' 缺少 Version 字段。"
	}

	$currentVersion = [string]$CatalogEntry.Version
	if ($currentVersion -notmatch '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)$') {
		throw "模块 '$($CatalogEntry.ModuleName)' 的版本 '$currentVersion' 不是支持的三段式版本号。"
	}

	$major = [long]$Matches.major
	$minor = [long]$Matches.minor
	$patch = [long]$Matches.patch

	switch ($Bump) {
		'major' {
			$major++
			$minor = 0
			$patch = 0
		}
		'minor' {
			$minor++
			$patch = 0
		}
		'patch' {
			$patch++
		}
	}

	return "$major.$minor.$patch"
}

$scriptRoot = $PSScriptRoot
$projectRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot '..')).Path
$modulesRoot = Join-Path $projectRoot 'src\Modules'
$frontendModulesRoot = Join-Path $projectRoot 'src\ClientApp\WebApp\src\app\modules'
$packageRoot = Join-Path $projectRoot 'package_modules'
$modulesJsonPath = Join-Path $projectRoot 'modules.json'
$serviceName = 'AdminService'

if (-not (Test-Path -LiteralPath $modulesRoot -PathType Container)) {
	throw "模块目录不存在：$modulesRoot"
}

if (-not (Test-Path -LiteralPath $packageRoot -PathType Container)) {
	New-Item -Path $packageRoot -ItemType Directory | Out-Null
}

$allModuleDirectories = @(
	Get-ChildItem -LiteralPath $modulesRoot -Directory |
		Where-Object { $_.Name.EndsWith('Mod', [StringComparison]::Ordinal) } |
		Sort-Object Name
)

if ($allModuleDirectories.Count -eq 0) {
	'[]' | Set-Content -LiteralPath $modulesJsonPath -Encoding UTF8
	Write-Warning '未找到任何以 Mod 结尾的模块，已输出空的 modules.json。'
	return
}

$catalogBeforePack = @(Read-ModuleCatalog -CatalogPath $modulesJsonPath)
$hasExplicitModuleSelection = $null -ne $Modules -and $Modules.Count -gt 0
$selectedModuleNames = [System.Collections.Generic.List[string]]::new()

if ($hasExplicitModuleSelection) {
	foreach ($requestedModuleName in $Modules) {
		foreach ($requestedName in $requestedModuleName -split ',') {
			$normalizedName = $requestedName.Trim()
			if ([string]::IsNullOrWhiteSpace($normalizedName)) {
				continue
			}

			$moduleDirectory = $allModuleDirectories |
				Where-Object { $_.Name -eq $normalizedName } |
				Select-Object -First 1
			if ($null -eq $moduleDirectory) {
				throw "未找到模块 '$normalizedName'。"
			}

			if (-not $selectedModuleNames.Contains($moduleDirectory.Name)) {
				$selectedModuleNames.Add($moduleDirectory.Name)
			}
		}
	}

	if ($selectedModuleNames.Count -eq 0) {
		throw '未提供有效的模块名称。'
	}
}
else {
	foreach ($moduleDirectory in $allModuleDirectories) {
		$selectedModuleNames.Add($moduleDirectory.Name)
	}
}

if ($hasExplicitModuleSelection) {
	$unselectedNewModules = @(
		$allModuleDirectories |
			Where-Object {
				-not $selectedModuleNames.Contains($_.Name) -and
				$null -eq (Find-ModuleCatalogEntry -Catalog $catalogBeforePack -ModuleName $_.Name)
			}
	)
	if ($unselectedNewModules.Count -gt 0) {
		$unselectedNames = ($unselectedNewModules.Name | Sort-Object) -join ', '
		throw "发现尚未登记到 modules.json 的新模块：$unselectedNames。请将其加入 -Modules 后再打包。"
	}
}

$moduleDirectories = @(
	$allModuleDirectories |
		Where-Object { $selectedModuleNames.Contains($_.Name) }
)
$metadataByModule = @{}
foreach ($catalogEntry in $catalogBeforePack) {
	if (-not [string]::IsNullOrWhiteSpace([string]$catalogEntry.ModuleName)) {
		$metadataByModule[[string]$catalogEntry.ModuleName] = $catalogEntry
	}
}

$targetVersions = @{}
foreach ($moduleDirectory in $moduleDirectories) {
	$catalogEntry = Find-ModuleCatalogEntry -Catalog $catalogBeforePack -ModuleName $moduleDirectory.Name
	$shouldBump = $Bump -ne 'none' -and $selectedModuleNames.Contains($moduleDirectory.Name)
	$targetVersions[$moduleDirectory.Name] = Get-ModuleVersion -CatalogEntry $catalogEntry -Bump ($shouldBump ? $Bump : 'none')
}

if (-not (Get-Command perigon -ErrorAction SilentlyContinue)) {
	throw '未找到 perigon 命令，请先安装支持 module pack --version 的 Perigon.CLI。'
}

$packHelp = (& perigon module pack -h 2>&1 | Out-String)
if ($LASTEXITCODE -ne 0 -or $packHelp -notmatch '(?m)--version\s+<VERSION>') {
	throw '当前 Perigon.CLI 不支持 module pack --version，请更新 CLI 后重试。'
}

$location = Get-Location
$failedModules = [System.Collections.Generic.List[string]]::new()

try {
	Set-Location -LiteralPath $projectRoot

	foreach ($moduleDirectory in $moduleDirectories) {
		$moduleName = $moduleDirectory.Name
		$frontendModuleName = $moduleName.Substring(0, $moduleName.Length - 'Mod'.Length).ToLowerInvariant()
		$frontendPath = Join-Path $frontendModulesRoot $frontendModuleName
		$packArguments = @(
			'module',
			'pack',
			$moduleName,
			$serviceName,
			'--version',
			$targetVersions[$moduleName]
		)
		if (Test-Path -LiteralPath $frontendPath -PathType Container) {
			$packArguments += @('--front-path', $frontendPath)
		}
		else {
			Write-Warning "模块 '$moduleName' 未找到对应前端目录：$frontendPath，将仅打包后端内容。"
		}

		$packStartTime = [datetime]::UtcNow
		Write-Host "开始打包模块：$moduleName (Version: $($targetVersions[$moduleName]), Service: $serviceName)" -ForegroundColor Cyan

		if ($PSCmdlet.ShouldProcess($moduleName, "执行 perigon $($packArguments -join ' ')")) {
			try {
				& perigon @packArguments
				if ($LASTEXITCODE -ne 0) {
					Write-Error "perigon $($packArguments -join ' ') 执行失败，退出码：$LASTEXITCODE"
					$failedModules.Add($moduleName)
					continue
				}

				$packageFile = Get-LatestModulePackage -PackageRoot $packageRoot -ModuleName $moduleName -PackStartTime $packStartTime
				if (-not $packageFile) {
					Write-Error "未在目录 '$packageRoot' 中找到模块 '$moduleName' 的打包结果。"
					$failedModules.Add($moduleName)
					continue
				}

				$metadata = Get-MetadataFromZip -ZipPath $packageFile.FullName
				if ($metadata.ModuleName -ne $moduleName) {
					throw "打包结果的 ModuleName '$($metadata.ModuleName)' 与预期 '$moduleName' 不一致。"
				}
				if ($metadata.Version -ne $targetVersions[$moduleName]) {
					throw "模块 '$moduleName' 的打包版本 '$($metadata.Version)' 与预期 '$($targetVersions[$moduleName])' 不一致。"
				}

				$metadataByModule[$moduleName] = $metadata
				Write-Host "  ✓ 已生成：$($packageFile.Name)" -ForegroundColor Green
			}
			catch {
				Write-Error "模块 '$moduleName' 打包失败：$($_.Exception.Message)"
				$failedModules.Add($moduleName)
				continue
			}
		}
	}
}
finally {
	Set-Location $location
}

if ($WhatIfPreference) {
	Write-Host 'WhatIf 模式：未生成模块包，也未更新 modules.json。' -ForegroundColor Yellow
	return
}

if ($failedModules.Count -gt 0) {
	$failedModuleNames = ($failedModules | Select-Object -Unique | Sort-Object) -join ', '
	throw "以下模块打包失败，未更新 modules.json：$failedModuleNames"
}

$sortedMetadata = @(
	$allModuleDirectories |
		ForEach-Object {
			if (-not $metadataByModule.ContainsKey($_.Name)) {
				throw "模块 '$($_.Name)' 没有可写入 modules.json 的元数据。"
			}

			$metadataByModule[$_.Name]
		} |
		Sort-Object ModuleName
)

if ($PSCmdlet.ShouldProcess($modulesJsonPath, '写入模块元数据汇总')) {
	$sortedMetadata | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $modulesJsonPath -Encoding UTF8
}

Write-Host "完成，共处理 $($moduleDirectories.Count) 个模块。" -ForegroundColor Green
Write-Host "压缩包输出目录：$packageRoot" -ForegroundColor Green
Write-Host "元数据汇总文件：$modulesJsonPath" -ForegroundColor Green
