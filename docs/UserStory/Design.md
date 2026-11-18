# Product Design Index

## Architecture Decisions

Modules 使用单个 PostgreSQL 业务库，不再解析租户或按租户分库。ApiService 完成表结构迁移后，调用 SystemMod 的方法初始化默认管理员；现有账号保留。

| Iteration | Product Design | Decision |
|---|---|---|
| `Iter1-EntityRelationship` | [PD0001-EntityRelationship](Iter1-EntityRelationship/PD0001-EntityRelationship.md) | 使用 Perigon.PostgreSQL 0.2.2；PostgreSQL-only；关系使用显式关联实体；资源搜索与收藏分页在数据库执行；旧库必须人工核对后显式 baseline。 |
| `Iter2-NotificationAudit` | [PD0002-NotificationAudit](Iter2-NotificationAudit/PD0002-NotificationAudit.md) | 新增仅后端的 NotificationMod 与 AuditMod；站内消息按用户保存已读/未读状态并区分消息/公告类型；审核状态由服务端校验；不包含前端。 |
- 服务 HTTP JSON 元数据按实际请求体、handler 声明返回类型和直接 JSON 调用生成；OpenAPI 所需的非 JSON 类型作为显式兼容项。登录策略 JSON 使用 SystemMod 自有的源生成 Context。
