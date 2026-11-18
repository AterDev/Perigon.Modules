# MiniApi 测试、验证与运维

## 基础命令

```powershell
dotnet build MyProjectName.slnx -c Release
dotnet test --project tests/ApiTest/ApiTest.csproj --list-tests
dotnet test --project tests/ApiTest/ApiTest.csproj
```

当前模板没有独立单元测试项目；只有真实数据库、缓存、AppHost 或 HTTP 契约需要时才运行集成测试。集成测试使用 `[Category("Integration")]`，每个测试会话复用一次 AppHost。

测试数据库由 AppHost Testing 环境的 `MyProjectName_test` 决定。清理必须从实际连接字符串读取目标、精确断言测试库名、安全引用标识符，并与 `appsettings.Test.json` 一致。

## 影响矩阵

| 变化 | 最小验证 |
|---|---|
| 纯业务逻辑 | Release build + 相关测试或人工验收 |
| 实体/DbContext | build + 单测 + schema/migration diff；必要时集成测试 + AOT publish |
| Endpoint/请求响应模型 | build + OpenAPI/真实 HTTP JSON + API 集成测试 + AOT publish |
| JSON、反射、依赖、DI、EF 模型 | `native-aot` 完整 publish；必要时运行产物 |
| Angular/生成客户端 | `pnpm build` + 关键页面交互 |
| ServiceDefaults/AppHost | build + Aspire 资源状态和健康检查 |
| Dockerfile/发布脚本 | Native AOT publish + 镜像构建 + `/health`、`/alive` 和关键 endpoint |
| 配置/鉴权 | 目标环境配置检查 + 授权失败路径 |

Smoke test 只证明资源启动；业务测试必须断言状态、绑定、JSON、授权和持久化结果。无法运行基础设施或目标架构验证时，明确记录未验证项。

## AOT 与发布

- 使用 `native-aot` 判断变化是否敏感并执行 `linux-x64` 自包含 Native AOT publish。
- 审查 linker/AOT/RDG warning，不抑制未知警告来制造通过。
- 单服务镜像使用 `scripts/PublishDocker.ps1`。脚本、Dockerfile 或测试脚本为空占位时不得当作证据。
- MiniApi 没有内置迁移资源；部署管线在 API 启动前完成 schema 校验/变更，并准备备份、恢复和兼容策略。

## 上线检查

- Release build、测试、Native AOT publish 和目标镜像运行均已验证。
- JWT、数据库和第三方凭据由 Secret/环境注入；CORS 使用明确白名单。
- OpenAPI、JSON、健康端点、端口、globalization、证书和资源限额已确认。
- 数据库 schema、备份、恢复点和回滚镜像已准备。
- 发布失败停止放量，保存日志和部署描述，不即席破坏生产 schema。
