# 前端请求同步脚本示例
$location = Get-Location

perigon g request https://localhost:17002/openapi/v1.json ../src/ClientApp/WebApp/src/app

Set-Location $location
