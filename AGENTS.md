# Perigon.Modules 仓库协作指南

## 仓库定位

本仓库是 Perigon CLI 与 `Perigon.templates` 的官方模块开发仓库，用于开发、集成验证和打包由官方维护的模块。仓库中的 Web API、Angular 客户端、数据库、Aspire AppHost 与测试项目共同组成模块验证宿主，不应把这里仅理解为普通模板示例或文档仓库。

准确性优先。开始修改前先读取相关用户故事、现有同类模块和实际代码；当 README、旧指令与代码不一致时，以当前任务、当前代码和配置为准，并在交付说明中指出差异。

## 项目结构

- 前端: `src/ClientApp/WebApp`
  - 基础共享依赖: `src/ClientApp/WebApp/src/app/modules/share`
  - 业务前端模块: `src/ClientApp/WebApp/src/app/modules/{module}`
- 后端接口服务: `src/Services`
- 实体定义: `src/Definition/Entity`
- 业务逻辑: `src/Modules`，按模块划分
- Share 共享项目: `src/Definition/Share`
- 服务扩展: `src/Definition/ServiceDefaults`
- 文档位于 `docs/`
- `src/Perigon`：从 `Perigon.template` 同步的基础库与源生成器。模块任务不要顺手修改这里，除非任务明确涉及框架能力。
- `test/ApiTest`：TUnit + Aspire.Hosting.Testing 的黑盒集成测试。
- `scripts`：迁移、请求客户端生成、菜单同步、基础库同步和模块打包脚本。
- `package_modules`、`modules.json`：已生成模块包及目录元数据；优先通过打包流程更新，不直接编辑 zip。

当前技术基线由仓库配置决定：.NET 10、C# 14、EF Core 10、Aspire 13、Angular 21、TypeScript 5.9、TUnit 和 Microsoft.Testing.Platform。NuGet 版本集中维护在 `Directory.Packages.props`，测试运行器由 `global.json` 指定。

## 工具优先级

- 涉及项目脚手架、模块或服务添加、代码生成、OpenAPI 客户端生成、MCP 配置时，优先使用 `Perigon` 相关能力。
- 涉及分布式应用启动、资源状态检查、日志链路排查、集成配置时，优先使用 `Aspire` 相关能力；普通构建和测试优先使用 `dotnet build` 或 `dotnet test`。
- 需要前端功能验证时，优先结合 Playwright 或前端构建校验。
- 新增或修改前端模块时，复用 `src/app/modules/share` 中的基础组件、守卫、管道、i18n 与 Material 聚合导入；不要重新创建顶层 `src/app/share`。

## Skill 路由

- `.agents/skills/perigon/SKILL.md`：Perigon 主入口。涉及脚手架、模块/服务生成、代码生成、OpenAPI 客户端、MCP、Studio，以及遵循 Perigon 约定的前后端开发时使用。
  - `references/module.md`：官方模块的创建、接线、元数据、兼容性与打包。
  - `references/backend.md`：实体、DbContext、DTO、Manager、Controller、迁移和后端服务模式。
  - `references/angular.md`：Angular 页面、路由、生成客户端、菜单、国际化与主题。
  - `references/perigon-cli.md`：Perigon CLI、MCP、Studio 和生成命令。
- `.agents/skills/aspire/SKILL.md`：Aspire 分布式应用的启动、资源操作、AppHost 和运行时工作流；需要启动、停止、等待、重建或诊断资源时使用。
- `.agents/skills/aspire-orchestration/SKILL.md`：Aspire 生命周期、资源级 rebuild/restart、锁文件和端口冲突处理。
- `.agents/skills/code-review/SKILL.md`：代码审查、质量门禁、安全风险、性能和架构一致性检查。
- `.agents/skills/dotnet-inspect/SKILL.md`：查询 .NET/NuGet/本地程序集 API，比较版本和定位类型实现。
- `.agents/skills/test/SKILL.md`：TUnit 单元测试、Aspire 托管 API 集成测试和测试环境诊断。

只读取与任务相关的 skill。完整模块任务通常从 `perigon` 开始，再按范围读取 `references/module.md`、`references/backend.md`、`references/angular.md` 和 `test`；涉及运行时或 AppHost 时追加 `aspire`/`aspire-orchestration`。旧的独立 `engineer`、`module`、`backend` 和 `angular` skill 已并入 Perigon 或删除，不要再引用旧路径。

## 思维模型

作为以目标为导向的架构师编写代码：

1. 命名清晰、简洁、易理解，充分利用语言的表达能力和类型系统，写出结构清晰、可维护的代码。
2. 代码结构清晰，模块划分合理，以面向对象思维定义和组织类和方法。
3. 方法和类符合单一职责原则，并补充必要说明。
