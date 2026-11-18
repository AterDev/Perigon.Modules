# MiniApi 项目脚本

先读取脚本参数与实现再执行；占位、空文件或文档示例不能当作有效验证。

## `scripts/CleanBinObj.ps1`

清理 `bin`、`obj`、`.vs` 等生成目录。执行前确认工作树并保留源码、迁移、配置和用户资产。

## `scripts/PublishDocker.ps1`

用于单个 ApiService 的 `linux-x64` 自包含 Native AOT 发布和 Docker 镜像构建。Dockerfile 基于 Ubuntu 24.04 Noble 的 `mcr.microsoft.com/dotnet/runtime-deps:10.0-noble`，与 glibc `linux-x64` 运行时匹配：

```powershell
./scripts/PublishDocker.ps1 -Service ApiService -ImageName myprojectname-api-service
```

- 运行前检查 Docker/Podman、目标 RID、项目 restore、Dockerfile 和输出目录。
- `-NoRestore` 只在目标 RID 已恢复时使用。
- 只有确需服务端字体渲染时才使用 `-InstallFonts`；该选项通过 Ubuntu `apt` 安装 `fontconfig`、`fonts-dejavu-core`、`fonts-noto-color-emoji` 以及一个 CJK 字体包。默认包为 `fonts-wqy-zenhei`，也可选择 `fonts-noto-cjk`。
- 成功构建后必须启动镜像并检查 `/health`、`/alive` 和受影响 endpoint；镜像存在不代表运行正确。
- 脚本会删除临时 publish 目录，执行时不要把用户文件放入该目录。

## `UpdateMenus.ps1`

脚本包含项目占位 key 和 URL。执行前核对目标服务、路由、认证、环境和差异；MiniApi 默认没有 AdminService，未安装对应后台能力时不要执行。

## Schema 与 OpenAPI

MiniApi 没有内置 `EFMigrations.ps1` 或 `GenSwagger.ps1`。数据库 schema 使用项目选定的独立管线/工具管理；OpenAPI 从运行中的 `/openapi/v1.json` 获取。任何外部写操作必须先确认目标环境。

## AOT 验证占位

若 `TestAotDockerBuild.ps1` 或 `Dockerfile.aot-test` 为空，不能视为测试入口。使用 `native-aot` 中的真实 `dotnet publish`，再执行 Docker 构建/启动；若基线失败，在 PT 记录首个根因和 blocker。
