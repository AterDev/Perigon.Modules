---
name: test
description: 为 MiniApi 规划、编写、运行和诊断 TUnit 单元测试、Aspire.Hosting.Testing API 集成测试及 Native AOT 发布验证。用于验收场景、Minimal API 契约、序列化、AppHost fixture、测试数据库、AOT 回归或完成证据；静态文档改动不要启动 Aspire。
---

# MiniApi 测试

使用 `global.json` 配置的 Microsoft.Testing.Platform 和 TUnit，不套用 VSTest 的 `--filter` 语法。先将需求场景和高风险设计决定映射到测试，再选择最小充分层级。

## 分层

- 可选的 `tests/UnitTest`：若项目新增纯验证、转换或计算逻辑，可按需创建该项目；当前模板不包含独立单元测试项目。
- `tests/ApiTest`：Minimal API 路由/绑定、JSON、OpenAPI、鉴权授权、Manager/EF、真实 AppHost 资源和服务集成。
- Native AOT：AOT-sensitive 变化使用 `native-aot` 执行 Release publish；必要时运行二进制或容器验证健康端点和真实 API 契约。
- 静态结构检查：文档链接、包内容、模板文件或规则不变式；不因为名称是“测试”就启动 Aspire。

每个强制验收场景至少对应一项自动测试或有理由的人工验收。AOT-sensitive 场景不能只在 JIT 测试宿主中证明。

## 编写测试

- 用可观察行为和期望结果命名。
- API 测试先断言状态码，再断言绑定、响应契约和持久化结果。
- 按风险覆盖认证、授权、验证、not-found/conflict、重复请求和破坏性操作。
- 对请求/响应 DTO、converter 或多态变化，覆盖真实 JSON 输入输出；不要只直接调用 handler。
- 高风险回归或明确 TDD 任务先让测试以预期原因失败，再实现并通过。
- 不留无意义断言、注释断言或恒通过占位测试。当前模板 smoke test 仅证明 AppHost 启动，不证明 endpoint 或 AOT 正确。

## Aspire fixture 与数据库安全

`GlobalHooks` 通过 `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` 每个测试会话启动一次 AppHost，并设置 `ASPIRE_ENVIRONMENT=Testing`。复用该应用，不要每个测试重启容器；发送请求前等待 `ApiService` 和依赖资源达到目标状态。

AppHost 是测试数据库名的事实源：Testing 环境使用 `MyProjectName_test`。清理逻辑从 AppHost 实际连接字符串读取数据库名，删除前必须精确断言目标是 `MyProjectName_test`，随后切换到 `postgres` 管理库并安全引用标识符。`appsettings.Test.json` 必须保持一致。不得绕过断言或指向开发/生产库。

## 运行

在 `MiniApi` 根目录：

```powershell
dotnet test --project tests/ApiTest/ApiTest.csproj --list-tests
dotnet test --project tests/ApiTest/ApiTest.csproj
```

聚焦选择使用 TUnit/MTP `--treenode-filter`。涉及 endpoint、DTO/JSON、依赖、反射、EF 或发布时，再按 `native-aot` 运行 Release build、Native AOT publish 和必要的运行验证。

## 失败分类与完成证据

保留首个有意义异常，区分：编译/发现、容器/DCP/readiness、数据库/认证/清理、业务回归、AOT/Trim/RDG warning 或 publish、运行时序列化/绑定、断言通过后的清理失败。不能增加 retry、弱化断言或抑制未知 warning 制造绿灯。

每项证据记录场景/风险、测试层、精确命令、结果和未覆盖范围。“测试存在”“普通 build 通过”都不能代替相应测试或 AOT publish 通过。
