# Project Tracking

## Current Iteration

- Iteration: [Iter1-EntityRelationship](Iter1-EntityRelationship/PT0001-EntityRelationship.md)
- Overall status: `done`
- Scope: Perigon.PostgreSQL `0.2.2`、PostgreSQL-only 数据访问、租户安全和迁移基线
- Current task: `TASK-007` provider 升级与数据库侧过滤分页（已完成）
- Constraint: 不自动 baseline、不重建已有数据库、不删除测试或开发数据

## Iterations

| Iteration | PT | Status | Progress |
|---|---|---|---|
| [Iter1-EntityRelationship](Iter1-EntityRelationship/PT0001-EntityRelationship.md) | [PT0001](Iter1-EntityRelationship/PT0001-EntityRelationship.md) | done | 7/7 |

## Recent Implementation

- 三个模块和共享数据库上下文已切换到 `Perigon.PostgreSQL 0.2.2`，不再使用 EF Core 数据访问或 `SaveChanges`。
- `EntityFramework` 目录作为兼容路径保留，实际实现使用 PostgreSQL context、显式查询和显式更新。
- 租户和软删除过滤保留；租户级 Manager 必须有 `TenantId`，匿名控制器按需解析租户级 Manager。
- 多对多继续使用显式中间实体；初始迁移模型包含既有业务外键、约束名称和删除行为。
- `Article.LanguageTypeValue`、`Article.ContentTypeValue` 使用整数持久化，分别映射旧的 `LanguageType`、`ContentType` 列；枚举属性仅作为未映射包装。
- AppHost 使用 PostgreSQL migration resource；测试环境使用临时数据库容器，非测试环境保留数据卷。
- `Migrations.ps1`、`DatabaseUpdate.ps1` 已切换到 Perigon.PostgreSQL.Tools 0.2.2；旧 EF 脚本仅保留兼容入口。
- Resource 搜索的定义、值和标签过滤，以及用户收藏的资源可见性过滤、计数和分页，均下推到 PostgreSQL 执行；查询保留租户和软删除过滤。

## Verification Status

- Entity、CMSMod、ResourceMod、SystemMod、AdminService 和 AppHost 的构建已执行。
- migration list 能发现当前 PostgreSQL 初始 migration。
- 依赖升级后的完整业务测试通过 `26/26`，覆盖资源搜索、收藏分页、跨租户访问和软删除隔离。
- 旧 EF 数据库的 baseline 不自动执行，必须在备份和 schema 核对后由运维人员显式执行。
