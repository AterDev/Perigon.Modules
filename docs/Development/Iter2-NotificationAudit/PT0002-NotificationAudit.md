# 通知与审计模块实现计划

## 来源与状态

- Iteration: `Iter2-NotificationAudit`
- Plan Task: `PT0002-NotificationAudit`
- Source PD: [PD0002-NotificationAudit](../../UserStory/Iter2-NotificationAudit/PD0002-NotificationAudit.md)
- Status: `in-progress`

## 任务

- [ ] TASK-000 [ARCH-001] 完成 .NET 11、`Perigon.PostgreSQL 0.3.1` 和 Mapster 移除的架构升级基线，并同步相关 `.agents/skills`；代码和全解决方案构建已通过，但仍缺少必要本地业务/数据库验证。
- [ ] TASK-001 [REQ-001] 分析并抽取 notifier 的通知实体、业务逻辑和接口，适配本仓库的模块、租户和 DTO 约定；抽取代码已形成，来源能力和安全边界仍待独立验收。
- [ ] TASK-002 [REQ-002] 实现 `NotificationMod` 模块项目、实体、Manager、DTO、控制器和服务注册；代码已实现并构建通过，但接口鉴权、租户和出站安全尚未完成验证。
- [ ] TASK-003 [REQ-003] 实现站内消息及用户消息状态模型，覆盖消息/公告类型、列表、详情、未读统计和标记已读接口；代码和测试用例已形成，真实接口测试尚未执行。
- [ ] TASK-004 [REQ-004] 分析 `WorkflowMod` 审核实现并实现 `AuditMod` 的实体、业务服务、DTO、控制器和状态流转校验；代码已实现并构建通过，但状态流转和通知联动尚未完成真实数据库验证。
- [ ] TASK-005 [REQ-005] 将两个模块接入 AdminService、解决方案和模块元数据，不新增前端代码；接线已完成，但本地数据库迁移和接口行为运行时证据仍不足。
- [ ] TASK-006 [REQ-006] 补充后端验证，覆盖租户隔离、用户已读状态隔离、消息类型和审核状态转换。
- [ ] TASK-007 [REQ-006] 依据最终模型生成或更新 PostgreSQL 迁移，并完成构建、测试和接口发现验证。

## 实现边界

- 本计划不包含 Angular 页面、菜单、国际化或前端 API 客户端。
- 站内消息用于系统业务通知和公告，不实现用户间私信、会话或社交关系。
- 审核模块以来源 `WorkflowMod` 的业务语义为参考，最终实体边界和命名以当前仓库约定及可验证接口为准。

## 验证记录

- `dotnet build Perigon.Modules.slnx --no-restore -v:q` — passed，0 warnings、0 errors；NotificationMod、AuditMod、AdminService、ApiService、AppHost 和 ApiTest 均成功生成。
- `pwsh -NoProfile -File scripts/Migrations.ps1 List` — passed，发现 `20261030070033728_InitialPostgreSQL` 和 `20261117011052004_NotificationAudit`。
- 临时 `CompatibilityCheck` migration model diff — passed，生成的 Up/Down 为空且 snapshot 未发生变化，说明当前模型没有额外漂移。
- `dotnet test --project test/ApiTest/ApiTest.csproj --no-build --list-tests` — passed，仅发现 43 个测试；实际数据库/接口测试未执行，因此不构成测试套件通过证据。
- 临时 .NET 11 控制台检查 — passed，GitLab 不同事件标识生成不同事件键及不安全链接抑制行为符合预期；该检查不替代数据库测试或完整 API 测试。
- 新迁移包含 Notification 8 张表和 Audit 5 张表，但当前没有 `FOREIGN KEY/REFERENCES`；跨表和跨租户一致性仍依赖 Manager 校验，需单独评审租户复合外键方案。
- 本地 PostgreSQL 集成环境尚未执行，未取得真实数据库集成测试、迁移应用和接口行为验证；本迭代不以生产部署、容器发布或运维验证作为交付条件。

## 实现记录

### `2026-11-17` — `TASK-000`~`TASK-005` / 模块实现与基线复核

- Status: `in-progress`
- Implementation: NotificationMod 已提供站内消息、收件状态、Webhook 事件/投递和恢复服务；AuditMod 已提供审核流、记录、任务和状态推进，并写入站内通知。AuditMod Manager 直接使用 `Entity.NotificationMod` 实体，形成通知模型依赖。
- Code evidence: `src/Modules/NotificationMod/`、`src/Modules/AuditMod/`、`src/Definition/EntityFramework/AppDbContext/DefaultDbContext.NotificationAudit.cs`、`src/Definition/EntityFramework/PostgresMigrations/20261117011052004_NotificationAudit.cs`
- Verification: 全解决方案构建通过（0 warnings、0 errors）；迁移 list 和空漂移检查通过；API 测试仅发现 43 项，实际数据库/接口测试未执行；临时 .NET 11 控制台检查 GitLab 事件键与不安全链接处理通过。
- Remaining: 本地 PostgreSQL 集成尚未执行；Webhook 接收、签名、去重、外部投递以及 `NotificationRecoveryService` 的租约/重试/恢复仍未实测；13 张新表无数据库外键；`NotificationManager.cs:282-284` 允许配置外部 HTTP(S) endpoint，`NotificationDeliverySender.cs:13-50` 使用 Data Protection secret 和出站 HTTP，但 SSRF 防护和 HTTP 重试语义仍需代码审查与本地验证；TASK-000~TASK-007 均保持未完成。

## MiniApi / Native AOT 改造（2026-11-17）

- [ ] TASK-008 [ARCH-001] 将现有业务 Controller 转为 ApiService Endpoint Group，保留路由、鉴权和租户隔离；Verify: Debug/Release build 与接口契约测试。
- [ ] TASK-009 [ARCH-001] 启用 Native AOT、JSON 源生成和单 ApiService 宿主；逐项处理 AOT/Trim/RDG 警告；Verify: `dotnet publish src/Services/ApiService/ApiService.csproj -c Release -r win-x64 --self-contained true` 和发布产物运行检查。
- [ ] TASK-010 [REQ-003, REQ-004] 使用真实 PostgreSQL 验证通知收件隔离、Webhook 和审批事务/并发；Verify: `dotnet test --project test/ApiTest/ApiTest.csproj`。

当前实现为进行中；不得以 Debug 构建或 Native AOT 二进制生成替代无未解释警告及真实接口运行证据。此前 Windows AOT 发布生成了二进制，但有大量 IL2026/IL3050/IL2091 等裁剪警告；尝试用不可达的本地测试连接启动时因数据库不可用而终止。当前环境未发现 Docker、Podman 或本地 PostgreSQL。

### 2026-11-17 补充验证

- ApiService 已接入 Endpoint Group；FilterBase 移除 OrderBy，分页参数允许省略并默认 1/20，通用分页按 CreatedTime 倒序。客户端自定义排序不再受支持。
- UnitTest 13 项通过，包含真实 HTTP 参数绑定与请求校验；这些测试未覆盖真实数据库持久化、事务或并发。
- Windows Native AOT 曾成功生成二进制；修正索引表达式后临时迁移的 Up/Down 为空，模型快照一致。裁剪告警及 AOT 运行时功能仍需核验。
- 当前环境没有可用的 Docker 或 PostgreSQL，真实数据库的通知收件隔离与审核业务测试尚未执行；TASK-008 至 TASK-010 均继续保持进行中。
- 最终 Windows x64 Release AOT publish 成功，仍有 7 项 IL 警告：HybridCache JSON 回退、CORS 配置绑定、系统日志反射及框架程序集裁剪元数据；不能视为 Native AOT 验证完成。

### 2026-11-17 PostgreSQL 与生成器验证

- 使用 wslc 的 PostgreSQL 18 隔离库完成两条迁移；真实 HTTP 验证公告发布、两用户独立已读、审核流创建、提交和审批通过。
- 修复 provider 查询表达式问题；Guid[] 审核人字段的临时迁移 Up/Down 为空、快照一致。
- Perigon.CLI JSON Context 生成器修复异步包装及嵌套泛型参数，12 项测试通过；对本项目连续生成结果一致。
- UnitTest 13 项通过；Windows x64 AOT 发布成功，仍有 5 项框架 HybridCache/组件裁剪警告。完整 ApiTest 套件未完成，不计为通过。

### 二进制缓存调整

- 移除 GeneratedCacheJsonSerializer，字符串数组及枚举字典使用显式二进制序列化；string/byte[] 使用 HybridCache 内置处理。预先注册工厂以阻止默认 JSON factory，未知类型需显式注册序列化器。
- Redis 隔离键双实例读写验证通过，测试涵盖字符串数组与枚举字典；缓存键使用 bin-v1 避免读取旧 JSON 数据。模板 MiniApi 已同步。
- Windows Native AOT 发布成功；HybridCache 程序集仍有静态 JSON 回退代码分析告警，另有框架 IL2101，不能仅凭发布成功认定 AOT Redis 运行已验证。

- 按要求同步移除模块仓库和 MiniApi 模板的 HashCrypto JSON 加解密方法，以及 Extensions 动态投影和字典排序方法；上述公开 API 的调用方需迁移。UnitTest 18 项通过、2 项 Redis 集成测试按配置跳过。

### App JSON 上下文调整

- Share 新增 AppSerializerContext 管理非 HTTP JSON 类型，删除登录策略逐字段 DOM 转换；服务继续使用独立 ApiJsonSerializerContext。MiniApi 模板已同步并修复分页排序残留编译问题。
- Windows x64 强制 Native AOT 发布：模块宿主成功、19 条 IL 警告；模板成功、38 条 IL 警告。原生产物业务运行尚未复验。

### 服务级 JSON Context

- ApiService、AdminService 的 ApiJsonSerializerContext 与 AppSerializerContext 均放入各自 Serialization 目录；模块通过 IAppJsonSerializer 获取服务提供的生成元数据。MiniApi 模板同步。
- Perigon CLI 保留接口请求/响应发现，同时扫描直接 JsonSerializer 泛型调用；修复识别多个 Context 的问题，实际生成两次结果一致。CLI 定向测试 14 项通过；UnitTest 20 项通过、2 项 Redis 测试跳过。
- Windows x64 Native AOT 发布成功，19 条 IL 警告；发布后 CLI 生成结果已通过普通测试，原生产物业务运行待验证。

### AOT 警告处理和原生 OpenAPI 验证

- UniversalDbFactory 为 TContext 标注公共构造函数；SimpleHttp 和 JsonHelper 增加 JsonTypeInfo 重载，旧重载标记不兼容裁剪/AOT。
- 旧 MVC、Swagger 和 JWT 从共享 MiniApi 编译路径隔离到 AdminService；Minimal API 枚举描述由编译期源生成器和 OpenAPI schema transformer 提供，模板同步。超过 Int32 范围的枚举值当前触发 PAOPENAPI001 编译错误，需要扩展枚举元数据格式。
- Native AOT Windows x64 发布成功，残留 5 项 HybridCache 依赖警告。原生进程连接 wslc 隔离 PostgreSQL/Redis，健康检查通过，OpenAPI 文档 HTTP 200，图片上传 schema 为 multipart 二进制。未验证真实文件上传和完整业务套件。
- UnitTest 26 项通过、2 项 Redis 测试按配置跳过；MiniApi 模板 ApiService 构建通过。

- 后续按要求移除未使用的 UniversalDbFactory、共享 DI 注册及专属测试。ApiService 和 AdminService 仍通过 AppDbFactory 创建默认上下文；MiniApi 模板没有该通用工厂。解决方案构建通过（保留既有 CS8601）；UnitTest 24 项通过、2 项 Redis 测试按配置跳过。本次未重新发布 Native AOT。

- 移除已不在解决方案和 AppHost 中的 AdminService MVC 宿主、旧 Swagger 与未启用的 JWT 中间件。ApiService 继续使用标准 JWT Bearer 和官方 OpenAPI。旧 CLI 模块打包仅识别 Controllers，脚本现在提前报错以免生成缺少接口的包。解决方案构建通过（既有 CS8601）；删除的管线不在 ApiService 的 AOT 发布闭包内。
