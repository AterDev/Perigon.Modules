# Product Design Index

## Architecture Decisions

| Iteration | Product Design | Decision |
|---|---|---|
| `Iter1-EntityRelationship` | [PD0001-EntityRelationship](Iter1-EntityRelationship/PD0001-EntityRelationship.md) | 使用 Perigon.PostgreSQL 0.2.2；PostgreSQL-only；关系使用显式关联实体；资源搜索与收藏分页在数据库执行；旧库必须人工核对后显式 baseline。 |
| `Iter2-NotificationAudit` | [PD0002-NotificationAudit](Iter2-NotificationAudit/PD0002-NotificationAudit.md) | 新增仅后端的 NotificationMod 与 AuditMod；站内消息按用户保存已读/未读状态并区分消息/公告类型；审核状态由服务端校验；不包含前端。 |
