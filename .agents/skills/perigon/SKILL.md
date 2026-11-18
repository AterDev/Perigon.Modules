---
name: perigon
description: MiniApi 项目开发入口。用于 Perigon CLI/MCP、Minimal API Endpoint、Manager、模型、Angular 客户端、Native AOT、测试和发布验证；普通非 Perigon 项目或 ApiStandard 专属 Controller/模块工作流不要使用。
---

# Perigon

当前模板是 `MiniApi`：ASP.NET Core Minimal API + Native AOT + PostgreSQL。以当前仓库代码、CLI 实时帮助和脚本实现为准；不要套用 ApiStandard 的 Controller、AdminService、多数据库或迁移资源约定。

## 核心原则

- 追求通用、简捷、灵活：优先官方和主流技术，避免为设计模式而设计、过度抽象、过度封装第三方库。
- Perigon 负责脚手架、模块和代码生成；构建、测试、运行和分布式编排分别使用 `dotnet`、项目包管理器和 Aspire。
- 先生成骨架，再审查业务语义。生成结果必须检查 endpoint 静态绑定、请求/响应边界、授权、查询规模、OpenAPI 契约和 AOT/Trim 兼容，不能把生成成功当作完成。
- 先复用 `src/Perigon`、`Definition/Share`、`ServiceDefaults` 与现有模块能力，再增加新的封装或依赖。
- 变更后按影响面验证；涉及 endpoint、JSON、反射、依赖、数据库、鉴权或发布时，必须执行 AOT 和对应专项检查。

## 模板边界

- 业务逻辑和 DTO 位于 `src/Modules/<Module>`；ApiService 的 `Endpoints` 负责 HTTP 接口。模板使用 PostgreSQL，且不含内置 EF migration 资源。
- Endpoint group 继承 `RestEndpointBase` 并声明静态 `MapEndpoints`；Manager 和 Endpoint 注册由源码生成器完成。
- 不要假设存在 `AdminService`、Controller、Standard 生成器或多数据库切换。
- 新增能力必须检查 Request Delegate Generator、AOT、Trim、反射、依赖和序列化兼容性。

## Reference routing

| 任务 | 必读 reference |
|---|---|
| CLI、MCP、Studio、代码生成、模块安装/打包 | [references/perigon-cli.md](references/perigon-cli.md) |
| 选择模板、理解目录、服务注入、配置与 AppHost 边界 | [references/architecture.md](references/architecture.md) |
| 实体、DbContext、模型、Manager、Endpoint、缓存/日志/鉴权 | [references/backend.md](references/backend.md) |
| 安装外部业务模块（确认其支持 MiniApi/AOT 时） | [references/module.md](references/module.md) |
| Angular 页面、菜单、i18n 与生成客户端 | [references/angular.md](references/angular.md) |
| 清理、迁移、OpenAPI、菜单和镜像脚本 | [references/scripts.md](references/scripts.md) |
| 单元/集成测试、验证矩阵、发布与生产检查 | [references/testing-operations.md](references/testing-operations.md) |

只读取当前任务需要的 reference。涉及 Aspire 生命周期、资源、日志或部署时使用环境中可用的 Aspire skill；涉及测试时读取 `test`；涉及 endpoint、JSON、反射、依赖或发布时读取 `native-aot`。

## 默认工作流

1. 识别受影响的 Endpoint/Manager/Model/Service、数据与授权边界，以及 AOT 影响。
2. 检查现有实现和 Perigon 实时帮助；可生成的骨架优先由 CLI/MCP 生成。
3. 按 Minimal API 架构边界补齐业务代码，并审查静态绑定、公开契约和所有生成内容。
4. 若实体或公开接口变化，依次处理迁移/OpenAPI/客户端，不跳过契约同步。
5. 运行最小充分验证；AOT-sensitive 变更必须包含 Native AOT publish 和必要的运行验证，报告任何未执行项。
6. 任何 AI coding 后使用 `docs` 和 `delivery-loop` 同步当前 PT 的实现结果、验证/AOT 证据、当前个人月度进度和受影响 PD；文档未同步不属于完成。
