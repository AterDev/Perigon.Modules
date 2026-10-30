# PostgreSQL 模型与迁移改造计划

## 来源与范围

- Iteration: `Iter1-EntityRelationship`
- Plan Task: `PT0001-EntityRelationship`
- Source PD: [PD0001-EntityRelationship](../../UserStory/Iter1-EntityRelationship/PD0001-EntityRelationship.md)
- PostgreSQL provider: `Perigon.PostgreSQL 0.2.2`
- Status: `done`

目标是保持三个模块的目录和业务行为，移除 EF Core 数据访问，统一使用 PostgreSQL provider，同时保留租户隔离、软删除和显式中间表。

## 任务

- [x] TASK-001 [REQ-001] 将数据库访问迁移到 Perigon.PostgreSQL
  - `DefaultDbContext`、`AppDbFactory` 和 `ManagerBase` 保留原位置，底层使用 Perigon.PostgreSQL。
  - Manager 使用显式查询、插入和 `ExecuteUpdateAsync`，不使用 `SaveChanges` 或实体跟踪。

- [x] TASK-002 [REQ-002] 保留租户和软删除安全边界
  - 租户实体使用全局租户过滤和软删除过滤。
  - 租户级 Manager 没有有效 `TenantId` 时拒绝创建。
  - 匿名控制器不得提前解析租户级 Manager，按需解析必须发生在认证和租户上下文建立之后。
  - AppHost 测试环境不挂载 PostgreSQL 数据卷，非测试环境保留数据卷。

- [x] TASK-003 [REQ-003] 保留显式关联表和外键行为
  - 多对多关系继续使用 `SysUserRole`、`SysMenuRole`、`SysUserOrganization` 等显式实体。
  - 迁移模型包含既有 29 条业务外键，并保留约束名和删除行为。

- [x] TASK-004 [REQ-004] 处理枚举整数持久化
  - `Article.LanguageTypeValue` 映射既有 `LanguageType` 列。
  - `Article.ContentTypeValue` 映射既有 `ContentType` 列。
  - 枚举包装属性标记 `[NotMapped]`，整数属性标记 `[JsonIgnore]`；服务端筛选和更新使用整数属性。

- [x] TASK-005 [REQ-005] 提供 PostgreSQL 迁移命令
  - `.config/dotnet-tools.json` 固定 `Perigon.PostgreSQL.Tools 0.2.2`。
  - `scripts/Migrations.ps1` 输出到 `src/Definition/EntityFramework/PostgresMigrations`。
  - `scripts/DatabaseUpdate.ps1` 支持新库 update 和显式 baseline，不自动 baseline。
  - `EFMigrations.ps1`、`EFUpdateDatabase.ps1` 仅作为兼容入口，不调用 `dotnet ef`。

- [x] TASK-006 [REQ-005] 完成真实 PostgreSQL 集成验证
  - 空库验证迁移 resource、初始 migration、默认租户 seed 和业务服务启动顺序。
  - 旧 EF 数据库仅在备份并核对 schema 后验证显式 baseline。

- [x] TASK-007 [REQ-006] 升级 PostgreSQL provider 并下推资源与收藏分页
  - Resource 搜索的定义、值和标签过滤使用 PostgreSQL 查询；用户收藏按资源可见性在数据库过滤，并由数据库执行计数和 `Skip/Take` 分页。
  - 完整业务测试 `26/26` 通过，覆盖资源搜索、收藏分页、跨租户访问和软删除隔离。
  - 依赖升级后的模块构建和数据库查询验证通过。
  - 旧 EF 数据库 baseline 仍需在实际接管前人工核对，不自动执行。

## 迁移与 baseline 操作

### 新数据库

```powershell
dotnet tool restore
pwsh .\scripts\Migrations.ps1 -Command List
pwsh .\scripts\DatabaseUpdate.ps1 -Connection "<connection-string>"
```

### 既有数据库

不得直接执行初始 migration，也不得使用 `EnsureCreated` 或删除/重建 schema。应先备份并核对表、列、主键、索引、外键和删除行为，再执行：

```powershell
pwsh .\scripts\DatabaseUpdate.ps1 `
  -Connection "<connection-string>" `
  -Baseline `
  -Migration "<verified-migration-id>"
```

如果模型元数据与旧库不一致，baseline 必须停止并由人工制定兼容 DDL 或模型调整方案。不同租户数据库必须分别执行验证和迁移。

## 验证记录

- `CMSMod`、`ResourceMod`、`SystemMod` 构建命令已执行。
- `AdminService` 和 `AppHost` 构建命令已执行。
- migration list 能发现当前 PostgreSQL 初始 migration。
- 依赖升级后的完整业务测试已通过 `26/26`，覆盖资源搜索、收藏分页、跨租户访问和软删除隔离。
- 解决方案构建为 `0` 警告、`0` 错误。
- 旧 EF 数据库 baseline 不属于自动测试范围；接管前仍需人工备份和 schema 核对。
