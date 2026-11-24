# 2026-11 项目进度

## Iter2-NotificationAudit — MiniApi / Native AOT

- 状态：in-progress；依据 `docs/Development/Iter2-NotificationAudit/PT0002-NotificationAudit.md` 的 TASK-008 至 TASK-010 执行。
- ApiService 正在迁移为 Minimal API 单宿主，保持租户隔离，现有通知和审核接口正在核验。
- Windows Native AOT 已生成二进制，但有大量裁剪警告；真实 PostgreSQL 与 HTTP 业务场景尚未执行。

- 2026-11-17：FilterBase 去除 OrderBy，分页缺省值的 HTTP 绑定已验证；UnitTest 13 项通过。索引改写后的临时迁移为空、快照一致。数据库集成和 AOT 裁剪告警仍待解决。
- 2026-11-17：Windows x64 Release AOT 发布成功，剩余 7 项 IL 裁剪警告；数据库业务集成尚未执行。
- 已在 wslc PostgreSQL 隔离库完成通知已读隔离及审核通过流程的 HTTP 验证；CLI JSON 生成器测试 12 项通过，Windows AOT 仍有 5 项框架警告；完整 ApiTest 未完成。
- 缓存改为非 JSON 二进制格式并同步 MiniApi；Redis 双实例读写通过。AOT 发布仍报告框架依赖静态裁剪警告，AOT 产物 Redis 运行待验证。
- 已同步移除 HashCrypto JSON 加解密与 Extensions 动态投影、字典排序方法；UnitTest 18 项通过、2 项跳过。
- 已将非 HTTP JSON 类型集中到 AppSerializerContext，移除逐字段 DOM 实现；Windows AOT 模块及模板发布成功，分别有 19 和 38 条 IL 警告。
- 各服务 JSON Context 已迁入 Serialization；CLI 保留接口类型发现并增加直接 JSON 调用扫描，定向 14 项通过、生成幂等；Windows AOT 发布仍有 19 项 IL 警告。
- AOT 项目代码告警已处理，Windows 发布剩余 5 条 HybridCache 依赖警告；原生产物 OpenAPI 文档和 multipart schema 已验证。单元测试 26 通过、2 跳过。
- 移除未使用的 UniversalDbFactory 及其共享注册和专属测试，保留 AppDbFactory。解决方案构建通过，单元测试 24 通过、2 跳过；本次未重新发布 Native AOT。
- Modules AppHost 默认引用已有 PostgreSQL 和 Redis（`ConnectionStrings:Default`、`ConnectionStrings:Cache`），日常启动不再创建容器；Memory 模式不引用 Redis。Testing 模式未提供连接时保留测试 PostgreSQL 容器并使用内存缓存。保留启动时的数据库迁移顺序。
- ApiService 启动时由 DatabaseInitializationWorker 应用 Perigon.PostgreSQL 迁移并初始化默认租户，随后由现有模块初始化服务补齐管理员和种子数据。移除 MigrationMode 参数和 AppHost 独立迁移资源。
- 2026-11-18：移除运行时租户机制并建立单库迁移基线。复用 wslc 中的 PostgreSQL 和 Redis 验证 ApiService 首次及重复启动；确认无 tenant_id 列，默认管理员仍为同一条记录。旧库保留。
- 2026-11-18：收紧 Minimal API JSON 契约，移除通用 AppJsonSerializer。AOT 实测 multipart 上传不依赖 IFormFile JSON 元数据，但 OpenAPI 依赖，因此仅在 ApiService 主 Context 中保留兼容声明。
- 2026-11-18：ApiJsonSerializerContext 由 443 项收敛至 175 项。Windows x64 AOT 已验证 OpenAPI、认证图片上传、审核列表和通知未读数。
- 2026-11-18：完成 PT0002 TASK-011。合并 DefaultDbContext、采用默认表名并移除旧迁移模型补丁。解决方案构建和格式检查通过，PostgreSQL 18 实测升级及回滚保留样例数据。Windows x64 Native AOT 发布、空白隔离库启动和健康检查通过，保留 HybridCache 既有警告。
- 2026-11-18：完成 PT0002 TASK-012。类库集中配置基类软删除，唯一索引自动限定未删除行，Modules 使用默认索引名和单行自动属性。类库 343 项单元测试和 103 项 PostgreSQL 集成测试通过，Windows x64 Native AOT 实测软删除及索引，迁移差异为空。运行时和 CLI 使用本地 0.3.2-preview.2。
- 数据库初始化失败时保留错误日志并继续启动，隔离库复现迁移历史冲突后 /alive 返回 200。
- ApiService 统一安排迁移和模块初始化。ResourceMod 种子方法接受现有上下文，在迁移后执行；隔离 PostgreSQL 首次和重复启动通过。
- 模块种子方法统一放在模块根目录 InitModule.cs。
- PostgreSQL 0.3.3 本地验证通过。迁移可直接从缺失目标库开始，ApiService 首次及重复启动均通过。
- Modules 适配 Perigon.PostgreSQL 0.3.4 的生成 DbSet 和命名约定。迁移探测确认表、列和索引保持稳定，31 个新增发现的外键待独立审查。
- Modules 使用 AddDbContextPool 与 options-only 上下文。Perigon.PostgreSQL 0.3.5 本地包已验证离线生成迁移、显式连接更新和原生启动。
- MiniApi 管线入口收敛至 WebExtensions，与模板方法名一致。Release 构建、隔离 PostgreSQL 启动和 OpenAPI 验证通过。
- 本地 PostgreSQL 迁移工具升级至 0.3.6，并验证两个版本查询选项。
- 修正 PascalCase 初始迁移的两个审核索引列名，隔离 PostgreSQL 验证启动和种子数据。
- 重建本地 perigon_modules，移除审核流启用状态部分索引。Aspire 和 PostgreSQL 验证通过。
- 2026-11-19：PT0002 TASK-013 至 TASK-015 已完成，计划进度 5/16。新增审核和通知管理后台、个人待办及收件箱。Angular 生产构建、七项 PostgreSQL API 集成测试、Windows x64 Native AOT 发布、OpenAPI 和 Edge 端到端主流程通过。保留 HybridCache 既有警告，外部 Webhook 实际投递与 Linux 原生运行未验证。
- 2026-11-19：PT0002 TASK-016 已完成，计划进度 6/17。Modules 与 MiniApi 的前端对齐 Angular 22.3.0 和 TypeScript 6.0.3，冻结锁文件安装及生产构建通过。Modules 审核、通知主流程浏览器回归通过。Perigon 技能已加入前端模块拆分与 Angular Agent Skills 使用指引。
- 2026-11-19：PT0002 TASK-017 已完成，计划进度 7/18。通知管理的六个标签页分别管理表单、分页与查询状态，沿用现有入口。Angular 生产构建及 Edge 浏览器业务回归、标签页状态隔离验证通过。
- Perigon.CLI 10.2.2 AOT 包验证完成，CMS/Resource 成功打包，三个耦合模块被依赖检查拒绝；CMS 装入 MiniApi 后发现构建兼容问题。
- 使用本地 Perigon.templates 1.3.18-local.4 和 Perigon.CLI 10.2.3-local.4 创建全新 MiniApi。CLI 将 CMSMod、ResourceMod 安装到同一 ApiService，并自动生成 JSON 上下文。Debug 与 Windows x64 Native AOT 均通过隔离 PostgreSQL 冒烟测试，覆盖健康检查、OpenAPI、两个鉴权列表接口、ResourceMod 初始化和 NotMapped 列映射。原生发布仍有模板及 HybridCache 的剪裁分析警告。
- Modules 切换本地 Perigon.PostgreSQL 0.3.6 与迁移工具 0.3.7，移除枚举整数包装。隔离 PostgreSQL 验证 16 列原地转换、旧枚举值保留及越界回滚。Windows x64 Native AOT 发布、迁移与枚举接口读写通过。
- ResourceMod 使用本地 Perigon.PostgreSQL 0.3.7 的导航识别约定，移除导航上的 NotMapped。定义属性的 Sort 从关联记录映射至响应 DTO，保留现有响应字段。离线对比确认迁移快照不变。
- 2026-11-20：清理 CMS 和 System 实体的冗余映射特性。迁移为空，模型快照一致，Windows x64 Native AOT 实库和鉴权接口验证通过。
- 2026-11-20：补齐 ngx-markdown 动态依赖，恢复 Aspire 前端。冻结安装及 Angular 生产构建通过，Playwright 验证登录、18 个页面和 CMS Markdown 详情。
- 2026-11-20：新增系统组织树管理和可空 ExternalId，完成旧数据迁移、Windows x64 Native AOT、权限和 Playwright 增删改验证。
- 2026-11-20：修复组织与角色弹窗的多余滚动条，记录 Angular 弹窗规则。生产构建及四种视口的 Playwright 验证通过。

- 2026-11-20：统一 Angular Material 弹窗滚动规则，审查 27 个正文模板，Playwright 在四种视口验证 15 个代表性弹窗。
- 2026-11-20：Aspire NuGet 包与 AppHost SDK 升级至 17.0.0，解决方案还原和构建通过。
- 2026-11-20：AppHost 迁移至 Aspire 17 的路径式项目注册，重启后 API 与前端运行正常。
- 2026-11-20：用 Perigon.CLI 正式版重生成 AOT JSON 上下文和 CMSMod、ResourceMod 1.1.0。Win-x64 Native AOT 发布及模板安装构建通过，三个存在跨模块引用的独立包待处理。
- 2026-11-20：审核、通知的跨模块调用迁移到 ApiService Endpoint，Manager 使用 ID 和 DTO，7 项 PostgreSQL API 集成测试通过。

- 2026-11-24：ApiService 同步模板 OpenAPI 3.1 与 1.1.3 枚举 XML 元数据，移除旧本地生成器和 x-enumData；Windows x64 Native AOT 文档请求验证通过，细节见 PT0002。

- 2026-11-24：使用 nuget.org 单源及全新包缓存还原模块 ApiService，确认 1.1.3 公开包可生成 XML。

- 2026-11-24：发现现有 Perigon.CLI 客户端生成未识别 annotated enum；重新生成客户端前需更新 CLI 并验证。
