# Product Design Index

## Architecture Decisions

Modules 使用单个 PostgreSQL 业务库，不再解析租户或按租户分库。ApiService 完成表结构迁移后，调用 SystemMod 的方法初始化默认管理员；现有账号保留。

实体直接声明枚举属性，由 Perigon.PostgreSQL 默认存为 `smallint`。升级已有实体时保留数据库列名和枚举数值，使用迁移原地转换列类型。

| Iteration | Product Design | Decision |
|---|---|---|
| `Iter1-EntityRelationship` | [PD0001-EntityRelationship](Iter1-EntityRelationship/PD0001-EntityRelationship.md) | 使用 Perigon.PostgreSQL 0.2.2；PostgreSQL-only；关系使用显式关联实体；资源搜索与收藏分页在数据库执行；旧库必须人工核对后显式 baseline。 |
| `Iter2-NotificationAudit` | [PD0002-NotificationAudit](Iter2-NotificationAudit/PD0002-NotificationAudit.md) | NotificationMod 与 AuditMod 接入 Angular 后台。站内消息区分消息和公告，每位收件人单独保存阅读状态。审核流转与管理权限由服务端校验。 |
- 服务 HTTP JSON 元数据按实际请求体、handler 声明返回类型和直接 JSON 调用生成；OpenAPI 所需的非 JSON 类型作为显式兼容项。登录策略 JSON 使用 SystemMod 自有的源生成 Context。

- ApiService 使用 OpenAPI 3.1 和共享构建任务生成的 `enum-meta.generated.xml`；非 Flags 整数枚举通过 `oneOf`/`const` 描述，别名使用 `anyOf`。枚举元数据嵌入程序集，Native AOT 不在运行时反射枚举字段；公开的 `x-enumData` 旧扩展不再生成。
