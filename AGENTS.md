# Perigon.Modules 仓库协作指南

## 仓库定位

本仓库是 Perigon CLI 与 `Perigon.templates` 的官方模块开发仓库，用于开发、集成验证和打包由官方维护的模块。仓库中的 Web API、Angular 客户端、数据库、Aspire AppHost 与测试项目共同组成模块验证宿主，不应把这里仅理解为普通模板示例或文档仓库。

准确性优先。开始修改前先读取相关用户故事、现有同类模块和实际代码；当 README、旧指令与代码不一致时，以当前任务、当前代码和配置为准，并在交付说明中指出差异。

## 开始工作

1. 先运行 `git status --short`，保留用户已有修改，不回退、不覆盖无关工作。
2. 阅读 `README.md`，再按任务读取 `docs/UserStory/`、`docs/Development/` 中相关文档。空文档不能作为约束来源。
3. 选择一个现有模块作为可运行范例：简单 CRUD 参考 `CMSMod`，系统初始化、后台任务与复杂业务参考 `SystemMod`。
4. 使用 `rg` 定位真实调用链、生成器约定和相邻实现，不凭文件名猜测行为。
5. 新模块或跨层改动先使用 `.agents/skills/engineer/SKILL.md` 路由，再按范围读取对应 skill。

## 项目结构

- `src/Definition/Entity`：所有模块的实体，按 `{ModuleName}` 目录组织。
- `src/Definition/EntityFramework`：共享 DbContext、数据库工厂与 EF Core 迁移。
- `src/Definition/Share`：Manager/Controller 基类、共享模型、常量和异常。
- `src/Definition/ServiceDefaults`：服务、数据库、缓存、中间件与可观测性注册。
- `src/Modules/{Name}Mod`：模块程序集；包含 DTO、Manager、模块服务、初始化逻辑和后台任务。
- `src/Services/AdminService`：后台管理 API，也是当前模块打包所使用的宿主服务。
- `src/Services/ApiService`：面向普通用户的 API。
- `src/Services/MigrationService`：AppHost 启动时执行迁移与基础数据初始化。
- `src/AppHost`：Aspire 分布式应用入口，编排数据库、缓存、迁移和 API 服务。
- `src/ClientApp/WebApp`：Angular 21+ 验证客户端；可分发前端模块位于 `src/app/modules`，其中 `modules/share` 是其他前端模块的基础依赖。它不是当前后端 zip 模块包的默认组成部分；启用前端打包时应将该基础依赖一并包含。
- `src/Perigon`：从 `Perigon.template` 同步的基础库与源生成器。模块任务不要顺手修改这里，除非任务明确涉及框架能力。
- `test/ApiTest`：TUnit + Aspire.Hosting.Testing 的黑盒集成测试。
- `scripts`：迁移、请求客户端生成、菜单同步、基础库同步和模块打包脚本。
- `package_modules`、`modules.json`：已生成模块包及目录元数据；优先通过打包流程更新，不直接编辑 zip。

当前技术基线由仓库配置决定：.NET 10、C# 14、EF Core 10、Aspire 13、Angular 21、TypeScript 5.9、TUnit 和 Microsoft.Testing.Platform。NuGet 版本集中维护在 `Directory.Packages.props`，测试运行器由 `global.json` 指定。

## Skill 路由

- `.agents/skills/engineer/SKILL.md`：全局入口；任务分解、跨层实现、架构审查和 skill 选择。
- `.agents/skills/module/SKILL.md`：官方模块的创建、接线、元数据、兼容性与打包。
- `.agents/skills/backend/SKILL.md`：实体、DbContext、DTO、Manager、Controller、迁移和模块后端服务。
- `.agents/skills/angular/SKILL.md`：Angular 页面、路由、生成客户端、菜单、国际化与主题。
- `.agents/skills/test/SKILL.md`：TUnit 单元测试和 Aspire 托管的 API 集成测试。

只读取与任务相关的 skill。完整新模块通常按 `engineer -> module -> backend -> test` 使用；只有明确需要管理界面时才追加 `angular`。

## 模块边界与接线

模块名必须以 `Mod` 结尾，并保持程序集名、实体目录、控制器目录和命名空间一致。当前源生成器只识别被服务项目引用、程序集名以 `Mod` 结尾，并提供公共静态 `ModuleExtensions.Add{AssemblyName}(IHostApplicationBuilder)` 的模块程序集；它据此生成 `AddModules()`，并自动注册继承 `ManagerBase` 的 Manager。

一个常规后端模块切片通常包含：

1. `src/Definition/Entity/{Name}Mod` 中的实体；
2. `DefaultDbContext` 中相应的 `DbSet` 与必要的模型配置；
3. `src/Modules/{Name}Mod` 中的项目、`ModuleExtensions`、DTO 和 Manager；
4. 目标服务项目的模块 `ProjectReference`；
5. `src/Services/{TargetService}/Controllers/{Name}Mod` 中的 Controller；
6. 必要的迁移、初始化逻辑和 API 集成测试；
7. 通过 Perigon 打包流程生成的 `metadata.json` 与 zip，而不是手工拼装产物。

现有模块包表明默认包边界包含 `Entity/{Name}Mod`、`Modules/{Name}Mod`、`Controllers/{Name}Mod` 和 `metadata.json`。不要默认把宿主配置、迁移、Angular 页面或 `src/Perigon` 基础库打入模块；如模块需要这些内容，先确认 Perigon CLI 的打包契约。

## 开发约定

- 遵循 `.editorconfig`：文件范围命名空间、花括号、4 空格缩进、PascalCase 类型/成员、接口 `I` 前缀。
- C# 以可读性优先：单行最长 120 个字符；超过时按语义在链式调用、参数、条件或对象初始化器处分行。每行只保留一条语句，不把多个独立操作压缩在同一行。
- C# 方法一律使用花括号方法体，不使用 `=>` 表达式体方法；仅 Lambda 表达式、属性、索引器和访问器可按现有 `.editorconfig` 约定使用 `=>`。
- 启用 nullable；优先使用当前仓库已采用的主构造函数、集合表达式和异步 API。
- 实体继承合适的基类；字符串必须设置合理最大长度，金额/高精度数值显式配置精度，枚举值提供可读描述。
- 保持 Controller 薄：路由、输入验证、授权和 HTTP 结果留在 Controller；查询、事务和业务规则留在 Manager/领域服务。
- 使用现有 `ManagerBase`、DbFactory、Mapster、分页和租户过滤能力；先检查准确签名，不发明新的包装层或 `ApiResponse`。
- 查询默认优先投影、分页和无跟踪读取，避免无界列表、N+1 和不必要的 `Include`。
- 业务错误沿用 `BusinessException`/全局异常中间件与 ProblemDetails 约定。
- 跨 Manager 协作并非绝对禁止，但要保持在同一业务用例内，避免循环依赖；新增协调逻辑前先参考 `SystemMod` 的现有做法。
- 仅在行为、使用方式或模块契约变化时同步用户文档；不要为普通代码改动生成总结、进度或测试报告文件。

## 工具、迁移与验证

优先使用已配置且当前可用的 Perigon CLI/MCP 生成能力；调用前先查看实际帮助或工具描述，不假定旧指令中的工具名仍然存在。检查生成结果并按仓库约定补齐，不把生成代码视为无需审查的最终结果。

不要在未确认影响时运行会修改数据库、生成迁移、覆盖同步目录或重写包产物的脚本。`SyncPerigon.ps1` 使用镜像同步，尤其需要明确授权和干净的目标范围。

验证必须与改动风险匹配：

- 指令/skill 迁移：检查 frontmatter、路径、旧引用和 `git diff`，不需要启动 AppHost。
- 单个后端项目：优先针对项目的编译或静态诊断。
- 数据库/API/跨服务改动：使用 `test/ApiTest` 的 Aspire 集成路径；运行前确认容器运行时和测试数据库清理条件。
- Angular 改动：在 `src/ClientApp/WebApp` 使用 `pnpm` 的最小相关检查。
- 打包：仅在任务要求更新发布产物时运行，并核对 zip 内容与 `metadata.json`。

不要默认启动完整分布式应用、执行迁移、同步基础库或重打所有模块。若环境失败与代码回归无关，要保留原始错误并清楚区分。
