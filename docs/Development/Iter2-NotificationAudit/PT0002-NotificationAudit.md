# 通知与审计模块实现计划

## 来源与状态

- Iteration: `Iter2-NotificationAudit`
- Plan Task: `PT0002-NotificationAudit`
- Source PD: [PD0002-NotificationAudit](../../UserStory/Iter2-NotificationAudit/PD0002-NotificationAudit.md)
- Status: `in-progress`
- Progress: `7/18`。历史后端事项继续按各任务的验收边界跟踪。

## 任务

- [ ] TASK-000 [ARCH-001] 完成 .NET 11、`Perigon.PostgreSQL 0.3.1` 和 Mapster 移除的架构升级基线，并同步相关 `.agents/skills`；代码和全解决方案构建已通过，但仍缺少必要本地业务/数据库验证。
- [ ] TASK-001 [REQ-001] 分析并抽取 notifier 的通知实体、业务逻辑和接口，适配本仓库的模块、租户和 DTO 约定；抽取代码已形成，来源能力和安全边界仍待独立验收。
- [ ] TASK-002 [REQ-002] 实现 `NotificationMod` 模块项目、实体、Manager、DTO、控制器和服务注册；代码已实现并构建通过，但接口鉴权、租户和出站安全尚未完成验证。
- [ ] TASK-003 [REQ-003] 实现站内消息及用户消息状态模型，覆盖消息/公告类型、列表、详情、未读统计和标记已读接口；代码和测试用例已形成，真实接口测试尚未执行。
- [ ] TASK-004 [REQ-004] 分析 `WorkflowMod` 审核实现并实现 `AuditMod` 的实体、业务服务、DTO、控制器和状态流转校验；代码已实现并构建通过，但状态流转和通知联动尚未完成真实数据库验证。
- [ ] TASK-005 [REQ-005] 将两个模块接入 ApiService、解决方案和模块元数据；具体前端任务见 TASK-013 与 TASK-014。
- [ ] TASK-006 [REQ-006] 补充后端验证，覆盖租户隔离、用户已读状态隔离、消息类型和审核状态转换。
- [ ] TASK-007 [REQ-006] 依据最终模型生成或更新 PostgreSQL 迁移，并完成构建、测试和接口发现验证。

## 实现边界

- Angular 接入范围见 TASK-013 至 TASK-015。历史实现记录中的 AdminService 对应现有 ApiService，开发后台使用 ApiService 的 HTTPS 端口 17002。
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
- 单库改造：移除租户实体、租户请求解析、JWT 租户声明与业务租户过滤，保留角色和用户级授权。Webhook 地址变为 `/api/webhooks/{sourceId}`。新库使用独立的单库迁移基线；旧租户库不自动导入或合并。ApiService 应用迁移后显式调用 SystemMod 管理员初始化。
- JSON/AOT 契约收紧：接口使用准确的 HttpResults 声明，CLI 从真实请求体、声明响应和直接 JSON 调用收集根类型；登录策略的生成元数据由 SystemMod 持有。Windows x64 Native AOT 去掉 IFormFile 声明时，上传成功且字节一致，但 OpenAPI 返回 500（缺少 IFormFile 元数据），所以 ApiService 主 Context 保留这一项 OpenAPI 兼容声明。
- 生成结果：443 项收敛至 175 项；OpenAPI 另需展开查询参数的可空标量/枚举元数据，不登记整个查询 DTO。Perigon CLI 定向测试 14 项通过。当前 CLI 工作区加载不了 .NET 11 的 OpenAPI 枚举生成器；本次重新生成时临时提供 SDK 实际生成的源码，完成后移除。
- Windows x64 Native AOT 发布并启动通过。复用 wslc PostgreSQL/Redis，在隔离开发库验证 OpenAPI 200、multipart 文件 schema 为 string/binary、认证 PNG 上传的落盘字节一致、审核列表和通知未读数 HTTP 200。迁移加载同步到工作树实际的 Migrations 目录；不修改原开发数据库。
- 最终验证：解决方案构建成功；UnitTest 24 项通过、2 项 Redis 专项按环境开关跳过；CLI 定向用例 15 项通过（含 JSON body / query / multipart 区分）；改动文件 whitespace 与 IDE0011 检查通过。AOT 仍有原有 5 条 HybridCache 依赖警告。

### DefaultDbContext 模型约定整理

- [x] TASK-011 [ARCH-001, REQ-006] 将 40 个 DbSet 集中在 DefaultDbContext，使用 Perigon.PostgreSQL 默认表名和迁移模型。普通收件索引使用实体属性，有条件唯一索引保留过滤谓词。0.3.1 的命名软删除过滤器按具体实体类型注册，保留静态调用。
- 迁移：UseDefaultTableNames 原地重命名 13 张表和 40 个主键约束。初始迁移与新快照均无数据库外键。CLI 临时生成的模型差异为空。
- 验证：dotnet build Perigon.Modules.slnx 通过，仍有既有 CS8601。定向 dotnet format whitespace 和 IDE0011 检查通过。隔离 PostgreSQL 18 验证迁移、回滚和重新升级，确认样例数据、主键与部分索引。完整 ApiTest 未运行。
- Native AOT：dotnet publish src/Services/ApiService/ApiService.csproj -c Release -r win-x64 --self-contained true -p:PublishAot=true -p:PublishTrimmed=true 通过，仍有 HybridCache 相关 IL 警告。Windows 原生程序使用内存缓存和空白隔离库启动，/alive 和 /health 均返回 200。Linux x64 和业务接口套件未复测。

### 基类软删除与默认索引

- [x] TASK-012 [ARCH-001, REQ-006] 在 Perigon.PostgreSQL 中一次启用基类软删除，由最终映射模型为唯一索引追加未删除行条件。Modules 使用本地 0.3.2-preview.2 运行时与迁移工具，普通唯一索引只声明列和 IsUnique。审核流已启用、记录待审批仍使用各自的额外条件。索引名使用默认规则，实体自动属性使用单行。
- 迁移：当前工作树的迁移目录从空基线生成 Init 和快照。迁移 CLI 复算差异为空。Windows x64 原生 ApiService 在空白 PostgreSQL 18 隔离库创建 10 个默认命名的部分唯一索引，/alive 和 /health 均返回 200。
- 验证：dotnet build Perigon.Modules.slnx、定向 dotnet format whitespace 和 IDE0011 检查通过。类库单元测试 343 项、隔离 PostgreSQL 集成测试 103 项通过。类库 NativeAOT Smoke 真实验证派生实体过滤、具名绕过、额外索引条件及非唯一索引。ApiService 使用 dotnet publish src/Services/ApiService/ApiService.csproj -c Release -r win-x64 --self-contained true -p:PublishAot=true -p:PublishTrimmed=true 发布通过，仍有 HybridCache 既有裁剪警告。完整 Modules ApiTest 和 Linux x64 未运行。

- 数据库初始化失败时记录异常并继续启动。迁移失败时跳过默认管理员初始化。隔离 PostgreSQL 复现旧迁移历史，确认保留错误日志且 /alive 返回 200。ApiService Release 构建通过，未改变 EF 模型或 AOT 配置。

### 宿主负责数据库初始化与后台任务

- ResourceMod 将种子数据写入移到接收现有 DefaultDbContext 的静态方法。ApiService 迁移成功后调用，种子写入保留单事务。连接指向外部数据库时需要预先创建数据库。
- 隔离 PostgreSQL 空库首次启动和再次启动均通过，环境、标签和资源定义各为 3 条，重复启动未增加记录。
- SystemMod 的日志批量写入由模块普通方法提供，ApiService 注册并调度日志后台任务。Modules 源码中无模块自己定义或注册的宿主后台服务。
- Windows x64 Native AOT 发布通过，保留 HybridCache 既有裁剪警告。原生产物连接隔离空库，首次和重复启动种子计数均为 3/3/3。另以普通 Release 产物连接不存在的数据库，确认宿主记录 3D000，/alive 返回 200。
- ResourceMod 与 MiniApi CommonMod 的种子方法统一放在模块根目录 InitModule.cs，由各自宿主调用。
- Perigon.PostgreSQL 0.3.3 在迁移连接收到 3D000 时先创建空库，再执行迁移。Modules 和本机迁移工具升级至本地 0.3.3；真实 PostgreSQL 迁移集成测试 5 项通过，ApiService Release 在目标库缺失时启动并写入种子，重复启动无重复记录。

### Perigon.PostgreSQL 0.3.4 适配

- 运行时与仓库迁移工具升级至本地 0.3.4。DefaultDbContext 使用源生成器初始化 DbSet 自动属性，移除 Set 包装。显式选用 snake_case，并通过 Table 固定四个受复数规则影响的历史表名。
- 迁移探测未发现表、列、索引变化。新版模型识别出 31 个现有快照未记录的外键，需另行审查存量数据与约束后纳入迁移；本次保持迁移文件和快照。
- EntityFramework 构建、ApiService Release 构建通过。UnitTest 使用隔离构建目录运行，24 项通过、2 项 Redis 测试跳过。隔离 PostgreSQL 首次和再次启动通过，种子数据未重复。
- Windows x64 Native AOT 发布与原生产物在隔离 PostgreSQL 上首次和重复启动通过，保留 HybridCache 既有裁剪警告。

### 池化 DbContext 与离线迁移命令

- DefaultDbContext 只提供公开的 DbContextOptions 构造函数。AddDbContextPool 注册主上下文，启动迁移任务借用独立 DI scope，AppDbFactory 保留调用方自管生命周期的非池化创建方式。snake_case 由 EntityFramework 程序集声明，运行时与 CLI 共用。
- 本地 Perigon.PostgreSQL 0.3.5 支持 options 构造函数。迁移 List、Add、Script 均在无连接字符串时执行成功；database update 使用显式连接创建隔离 PostgreSQL 数据库并写入初始迁移历史。迁移探测仍只有此前记录的 31 个外键差异。
- ApiService Release 与 Windows x64 Native AOT 发布通过，保留既有 HybridCache 裁剪警告。隔离 PostgreSQL 验证首次和重复启动，种子数据无重复。类库迁移生成定向测试 12 项通过。
- ApiTest 项目在隔离输出目录编译通过。

### MiniApi WebExtensions 对齐

- 将 Modules 的中间件注册和管线入口集中到 WebExtensions，ApiService 使用模板的 AddMiddlewareServices 与 UseMiddlewareServices。现有注册内容与管线顺序保留。
- ApiService Release 构建及定向 whitespace 检查通过，隔离 PostgreSQL 首次和重复启动的 /alive、/openapi/v1.json 均返回 200。仅移动与改名，未改变依赖、映射或 AOT 约束，本次未重新发布原生产物。
- PostgreSQL 迁移工具本地与全局安装 0.3.6，验证 --version、-v 和 List。运行时包继续使用 0.3.5。
- 审核部分唯一索引的额外 SQL 谓词使用 PascalCase 物理列名 IsEnabled 和 StatusValue。同步未应用的初始迁移及快照。ApiService Release 构建及隔离 PostgreSQL 首次和重复启动通过，并确认迁移探测差异为空。
- 移除 AuditFlow 的启用场景部分唯一索引，保留 FlowKey 的软删除唯一性。AuditRecord 的额外状态谓词移到 Fluent API。按要求重建本地 perigon_modules 及初始迁移，Aspire ApiService Healthy，迁移历史、索引和资源种子已在 PostgreSQL 核实。本次未重发 Native AOT。

### Perigon.CLI AOT 模块打包验证

- 使用本机 Perigon.CLI 10.2.2 和 isAOT=true，在临时副本以 1.2.3 打包。CMSMod 包含 2 个 Endpoint，ResourceMod 包含 4 个 Endpoint 与 40 个 TypeScript 文件。两包元数据均为 1.2.3。
- AuditMod、NotificationMod、SystemMod 因跨模块依赖被 CLI 拒绝。CLI 模块打包和解包定向测试 11 项通过。
- CMSMod 安装到临时 MiniApi 模板时命令返回成功，但解决方案接线警告 Share.csproj 重复，构建因 NotMapped、Column 特性冲突和 JsonIgnore 缺失而失败，未能发布模板安装产物。
- 修正模板兼容性及 CLI 接线后，在新建的临时 MiniApi 安装 CMSMod 和 ResourceMod，Debug 和 Windows x64 Native AOT 发布产物均正常启动。隔离 PostgreSQL 验证种子数据、鉴权列表、OpenAPI multipart 文档，以及枚举存储列和非映射属性。
- PostgreSQL 库移除 IndexAttribute.Filter，显式谓词使用 Fluent HasFilter；生成器与反向脚手架同步。定向单测 31 项与真实 PostgreSQL 元数据测试 2 项通过。现有本地包版本尚未替换。

### 2026-11-19 审核与通知管理后台

- [x] TASK-013 [REQ-007, SC-007] 按角色接入审核侧栏、流程配置、记录和个人申请、待办处理。Angular 构建和浏览器审批流程通过。
- [x] TASK-014 [REQ-008, SC-008] 接入通知草稿、定向发布、收件箱、来源、接收器、规则、事件和投递记录。Angular 构建和浏览器发布、收件、接收器保存流程通过。
- [x] TASK-015 [REQ-007, REQ-008] 适配后台接口。提供管理员用户选择投影，修复角色查询、审核筛选及接收器持久化，补齐站内通知草稿修改契约。PostgreSQL API 测试、OpenAPI 与 Windows x64 Native AOT 验证通过。

- 实现位置为 `src/ClientApp/WebApp/src/app/modules/audit`、`src/ClientApp/WebApp/src/app/modules/notification` 与 `src/Services/ApiService/Endpoints/SystemMod/SysUserEndpoints.cs`。两个模块共享用户选择器及客户端基础服务。
- 草稿的收件人和过期时间可修改。发布在事务中锁定并读取当前草稿，按发布时的受众生成收件记录。当前 API 的状态码和兼容性保持原有契约，新增可选更新字段。
- Angular 生产构建通过，执行 `pnpm build --output-path <临时目录>`。Edge Playwright 使用原生 ApiService 和隔离 PostgreSQL，验证管理员和普通用户导航、审核人搜索、流程创建和审批、通知草稿定向发布、收件阅读，以及接收器新增和编辑。
- 浏览器另验证创建 Webhook 来源、展示接入路径、关联接收器创建规则、查看事件与投递列表，以及无外发的超时清理。
- 分别执行 `dotnet test -c AdminVerification --project test/ApiTest/ApiTest.csproj -- --treenode-filter "/*/*/<测试类>/*"`。AuditModApiTests 两项、NotificationModApiTests 五项通过。测试覆盖带状态及场景筛选、任务授权、申请撤回、修改草稿收件人与过期时间、已读隔离和接收器持久化。
- 执行 `dotnet publish src/Services/ApiService/ApiService.csproj -c Release -r win-x64 --self-contained true -o <临时目录>`。产物在隔离 PostgreSQL 启动，健康、OpenAPI 与业务 HTTP 验证通过。使用 Perigon 从该 OpenAPI 生成并核对 TypeScript 契约。变更文件 whitespace 与 IDE0011 检查通过。
- 保留 HybridCache 的既有 AOT/Trim 警告。当前验证覆盖 Windows x64。本次未向外部接收器发送 Webhook，也未执行 Linux x64 原生运行验证。
- 独立运行 ResourceModApiTests，13 项中 7 项通过。失败集中在资源定义删除的外键约束、资源错误响应格式及收藏清理，留在资源模块验证范围跟踪。

### 2026-11-19 Angular 22 与前端模块规范

- [x] TASK-016 [ARCH-002] 升级 Modules 和 MiniApi 的 Angular 前端，同步模块拆分规范。依赖锁定、生产构建与后台烟测通过。
- 结构检查覆盖顶层路由、模块目录和组件职责。审核模块按流程、记录、待办拆分。通知管理首页当时将多个用例集中在同一组件和表单，拆分见 TASK-017。规范写入 Perigon 的 Angular reference。
- Modules 和 MiniApi 的 Angular Framework、CLI、Build、CDK 和 Material 对齐至 22.3.0，TypeScript 对齐至 6.0.3。Modules 从锁定的 21.2.x 运行 Core、CLI、CDK 和 Material 迁移，保留原有组件变更检测和 XHR 行为，使用显式 `src/*` 路径映射。MiniApi 原来已在 22.x。
- 两处均通过 `pnpm exec ng version`、`pnpm build` 和冻结锁文件安装。Modules 执行 `pnpm install --frozen-lockfile --prefer-offline`，MiniApi 另加 `--ignore-scripts`。Modules 的 `pnpm exec tsc -p tsconfig.app.json --noEmit` 通过，Edge 在隔离 ApiService 上复测登录、菜单、审核、站内通知、接收器、规则、超时清理、资源懒加载与 CMS Markdown 渲染。
- Angular Build 的可选 WASM 解析器仍报告 emnapi 传递依赖的 peer 版本告警。Windows 原生构建通过。本次变更位于客户端和项目技能，ApiService 的 Native AOT 发布闭包不受影响。

### 2026-11-19 通知管理页按用例拆分

- [x] TASK-017 [REQ-008, SC-008] 保留通知管理菜单与标签页入口，分别组织站内通知、来源、接收器、规则、事件和投递记录。各用例管理查询、分页与相关表单状态。
- `src/ClientApp/WebApp/src/app/modules/notification/index` 负责标签页和页头刷新。六个独立组件按首次进入加载，分别持有表单、分页及请求状态。共享列表处理加载、空态、错误和翻页，操作复用现有确认弹窗和提示。
- `pnpm build --output-path <临时目录>` 通过。Edge Playwright 使用隔离 PostgreSQL 和已有 Native AOT ApiService，验证草稿编辑及收件人保留、定向发布、收件阅读、Webhook 来源创建、接收器新增与编辑、规则新增与编辑、取消及确认删除、事件和投递查询、超时清理。
- 浏览器验证从来源面板切至接收器再返回时保留未保存输入，来源每页条数及页码独立保留，页头刷新命中当前面板。外部 Webhook 发送未纳入本次浏览器测试。客户端拆分不影响 ApiService 的 Native AOT 发布闭包。

### 2026-11-19 枚举直接映射

- 本地安装 Perigon.PostgreSQL 0.3.6 与迁移工具 0.3.7。Modules 直接使用枚举属性，保留原有数据库列名和 API 枚举数值。
- 迁移 20261119083747390_StoreEnumsAsSmallint 原地转换 16 列。隔离 PostgreSQL 以旧迁移建库，验证 19 条样本记录的旧枚举值在升级、回滚和再次升级后均保留。越界值触发整条迁移回滚。
- Windows x64 Native AOT 发布后，从旧 schema 启动并自动迁移。健康检查、OpenAPI、CMS 和 Audit 的枚举筛选、Resource 和 System 的枚举读取、Resource 枚举新增和更新均通过。保留 HybridCache 剪裁警告。Modules 单元测试通过 24 项，Redis 测试按配置跳过 2 项。

### 2026-11-19 ResourceMod 导航与属性排序

- Perigon.PostgreSQL 0.3.7 识别引用和集合导航。ResourceMod 清理导航上的 NotMapped 和重复列名声明，保持列、索引和外键。离线模型快照相同。
- 资源属性的 Sort 由 ResDefinitionPropertyMap 投影至响应 DTO。Windows x64 Native AOT 实库验证三个资源定义的 17 个属性排序值、JSON 和 OpenAPI，ApiTest 项目构建通过。

### 2026-11-20 其余模块的实体映射清理

- CMSMod 和 SystemMod 移除冗余导航和同名列特性。保留 AuditMod 旧列名映射与 SysLogs.Data 排除映射。离线生成的 Up 和 Down 为空，模型快照逐字节一致。
- Windows x64 Native AOT 发布产物在隔离 PostgreSQL 启动。健康检查、OpenAPI、CMS 和 System 鉴权查询及 Resource 排序回归通过。HTTP 契约与数据库结构不变，产品设计无需调整。

### 2026-11-20 Aspire 前端恢复

- 补齐 ngx-markdown 动态导入使用的 marked-katex-extension，修复 Angular/Vite 开发服务器无法加载 CMS 模块的问题。前端冻结锁文件安装和生产构建通过。
- 重启 Aspire 的 frontend 资源后，Playwright 用本地管理员完成登录并覆盖 18 个页面、Resource 配置与通知管理标签页，以及 CMS 测试文章的创建、Markdown 渲染和清理。ApiService 健康，浏览器检查没有出现新错误。

### 2026-11-20 SystemMod 组织管理

- 系统菜单新增管理员组织管理。支持查看树、新增根组织及子组织、编辑和移动、删除无关联的组织。后端校验循环、同级重名、下级与关联用户约束。
- AddOrganizationExternalId 仅添加可空 varchar(40) 列。Windows x64 Native AOT 从旧 schema 启动迁移，保留已有组织记录。API 验证权限、组织增删改、40 字符边界和循环与下级约束。Playwright 验证实际组织页面添加子节点、编辑、清空外部 ID 和删除。
- Angular 生产构建、C# 格式与 IDE0011 检查通过。数据库验证使用隔离 PostgreSQL。

### 2026-11-20 系统表单弹窗滚动修复

- 新增组织、角色新增和角色编辑共用弹窗正文布局。表单随弹窗宽度收缩，校验提示按需占高，视口高度不足时正文滚动，操作区始终可见。同步 Perigon Angular 技能。
- Angular 生产构建通过。Playwright 覆盖四种视口，验证必填提示、滚动边界、窄屏及操作按钮可见性。字段契约与业务流程不变。

### 2026-11-20 Angular 弹窗统一滚动布局

- 排查 27 个 Material 弹窗正文。由全局样式统一约束视口高度和正文滚动，清理表单最小宽度、固定滚动区域与空白提示占位。同步 Perigon Angular 技能中的布局与验证规则。
- Playwright 在四种视口验证 15 个代表性弹窗，覆盖校验提示、长表单和嵌套选择器。高度不足时正文可滚动，标题和操作区保持可见，未出现横向滚动或弹窗外层滚动。

### 2026-11-20 Aspire 17.0

- Aspire NuGet 集中版本与 AppHost SDK 升至 17.0.0。NuGet 源版本核对及隔离产物的解决方案还原构建通过。现有 AppHost SDK 和项目资源写法在 Aspire 17 中有弃用警告。

### 2026-11-20 Aspire 17 AppHost 迁移

- AppHost 改用标准 .NET SDK，显式引用 Aspire Hosting AppHost 和 Dotnet 17.0.0，并按项目路径注册 ApiService。
- ApiTest 项目构建通过。重启 Aspire 后 ApiService、前端及两个连接字符串资源均由 Aspire 报告健康，API 健康检查与前端入口均返回 HTTP 200。

### 2026-11-20 AOT 模块重新生成

- 使用 NuGet 正式版 Perigon.CLI 10.2.2 重新生成 ApiService JSON 上下文，并使用 Perigon.PostgreSQL 0.3.8。
- CMSMod 和 ResourceMod 从 1.0.0 升至 1.1.0，ZIP 与目录元数据一致。官方脚本支持选择已有的可打包模块，目录只记录已有包和本次生成的包。
- AuditMod 引用 NotificationMod，NotificationMod 引用 SystemMod，SystemMod 的枚举目录引用 AuditMod、CMSMod 与 ResourceMod。CLI 拒绝将这三个模块打成独立包，现有 SystemMod 1.0.0 未替换。
- 使用公开模板 1.3.18 创建隔离项目，CLI 安装两个新包并生成 JSON 上下文，解决方案构建通过。ApiService Windows x64 Native AOT 发布通过，保留 HybridCache 依赖的裁剪和 AOT 分析警告。
- Windows x64 Native AOT 产物通过健康检查和 OpenAPI 测试，自动创建隔离 PostgreSQL 数据库并迁移 41 张表。验证结束后已清理临时数据库。

### 2026-11-20 跨模块调用收敛到 Endpoint

- 审核和通知 Manager 只接收用户 ID、显示名快照或通知 DTO。ApiService 的 Endpoint 调用 SystemMod 获取用户信息，审核通知由 Endpoint 接线到 NotificationMod，在审核事务中写入。
- SystemMod 只定义自己的枚举目录，外部枚举值由 ApiService 汇总。枚举目录使用缓存工厂生成有效值，修复首次未命中时尝试序列化空值的问题。没有实体和迁移变更。
- 审核 2 项、通知 5 项 API 集成测试在隔离 PostgreSQL 通过。现有 CLI 仍会拦截可打包目录里的跨模块 Endpoint，审核和通知没有出独立新包，SystemMod 保留原包。
- Windows x64 Native AOT 重新发布后，在隔离 PostgreSQL 验证健康检查、登录以及枚举目录首次和重复读取均通过。发布保留既有 HybridCache 裁剪警告。

### 2026-11-24 OpenAPI 枚举契约同步

- ApiService 切换 Perigon.AspNetCore.SourceGeneration 1.1.3，使用构建生成并嵌入的 `enum-meta.generated.xml`；移除本地 OpenAPI 源生成器和 `x-enumData`，按 OpenAPI 3.1 annotated enumerations 输出整数枚举，别名使用 `anyOf`。上述历史记录中的 PAOPENAPI001/Int32 限制已不适用。
- Debug 解决方案构建及单元测试通过（24 通过、2 跳过）；Windows x64 Native AOT 发布并以隔离无数据库连接的进程验证 `Sex` 的数字 const、成员名和中文描述。Linux/macOS 原生运行未验证；业务数据库路径不在本次验证范围。

- 公开 NuGet 源和全新包缓存还原 ApiService、Debug 构建及元数据生成通过；存在既有 SysUserManager CS8601 警告。

- 当前 Perigon.CLI 客户端生成仍以 `enum` 或 `x-enumData` 判断枚举并读取旧扩展；它尚不识别本次 `oneOf`/`const` 输出。既有生成客户端不变，但重新生成前需先完成 CLI 兼容更新并验证。

### 2026-11-26 ModuleDemand 打包与注册

- CLI 支持打包和安装 `ModuleDemand/<模块名>`，检查骨架的跨模块引用，安装时保留同名实现。CLI 打包与解包定向测试 15 项通过，隔离项目实际执行打包、安装、重装和构建通过。
- `ManagerSourceGen` 根据宿主目录和模块程序集名生成 scoped 注册。本仓库及模板继续引用公开版分析器，使用该功能需发布并升级分析器包。Audit 和 Notification 的现有业务接线待迁移。
- 使用本地分析器包，验证两个接口在同一 DI 作用域共享实例，在独立作用域分别实例化。隔离项目 Windows x64 Native AOT 发布及运行通过。

### 2026-11-26 移除 ApiService Composition

- 审核通知改由 `IAuditNotificationDemand` 转交宿主写入原审核事务。ApiService 对公开版 SourceGeneration 1.1.3 使用 `TryAddScoped` 补齐注册。安装骨架可在宿主没有接入通知时继续执行审核流程。
- 移除 `GET /api/SysConfig/enum`、手写枚举目录及无调用方的前端客户端。OpenAPI 继续读取 `enum-meta.generated.xml`。
- `dotnet test --project test/UnitTest/UnitTest.csproj --no-restore` 通过 24 项，Redis 专项跳过 2 项。前端 `pnpm exec tsc --noEmit -p tsconfig.app.json` 通过。
- ApiService Windows x64 Native AOT 发布成功，仍有 HybridCache 既有告警。原生宿主的 OpenAPI 中已无旧枚举路由，保留审核路由和来自 XML 的枚举注解。本轮未执行 PostgreSQL 审核通知链路测试，未执行 CLI AOT 验证。

### 2026-11-26 模块包发布验证

- Audit 和 Notification 用户查询改由各自 Demand 接口提供。验证宿主使用模块自己的委托接线，包内骨架无 SystemMod 等业务模块引用。Notification Webhook 使用模块自己的 JSON 源生成上下文。
- 本地 SourceGeneration 1.1.5 包在目录、实现类和模块接口名均匹配时才生成注册。使用本地 Perigon.templates 11.0.0 创建新 MiniApi 项目，安装 AuditMod 1.0.0 和 NotificationMod 1.0.0 并构建后端，确认 3 个 Demand 接口均自动注册。官方 ZIP 的元数据与目录版本一致。
- SystemMod 1.1.0 候选包安装到干净模板时缺少 Toolkit 的 `ImageHelper`。目录继续使用 1.0.0。Angular 源码打入 ZIP，未在独立模板中完成前端接入验收。
