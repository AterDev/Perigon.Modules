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
