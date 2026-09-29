# Entity Relationship Mapping and Migration Baseline

## 来源与范围
- Iteration: `Iter1-EntityRelationship`
- Plan Task: `PT0001-EntityRelationship`
- Source PD: [PD0001-EntityRelationship](../../UserStory/Iter1-EntityRelationship/PD0001-EntityRelationship.md)
- Status: `blocked`
- Progress: `2/3`
- Goal: 模块化 EF Core 映射并将迁移历史重建为当前模型的 PostgreSQL 空库基线。
- Included: EntityFramework 配置类、显式 join entity 关系、历史迁移重置和 PostgreSQL `Initial`。
- Excluded: 将迁移应用到现有数据库、已有影子 join 数据转换、SQL Server 基线生成。

## Plan
- 先把无法由约定表达的关系行为和 provider 默认值移入模块配置类。
- 删除历史 migration、designer 和旧 snapshot，再由当前模型生成 `Initial`。
- 编译 EntityFramework 并导出 `0 -> Initial` SQL；之后验证 AdminService 集成构建。
- 不在未备份/未制定数据转换方案时对已有数据库执行迁移。

## Tasks
- [x] TASK-001 [REQ-001] 将保留的 Fluent API 拆入模块配置类，并由 `DefaultDbContext` 程序集扫描自动应用
  - Depends on: none
  - Done when: `EntityConfigration/{ResourceMod,SystemMod}` 配置覆盖 Restrict 和字段默认值，EF 项目可构建；新增配置类无需手动注册。
  - Verify: `dotnet build src/Definition/EntityFramework/EntityFramework.csproj -v minimal` — passed。
- [x] TASK-002 [REQ-002] 删除旧 migration 历史并生成新的 PostgreSQL `Initial`
  - Depends on: TASK-001
  - Done when: migration 目录仅包含新 `Initial`、Designer 和 snapshot；`Up` 导出的 SQL 含完整建表内容且不重命名/删除旧对象，`Down` 仅回滚本迁移创建的对象。
  - Verify: `dotnet ef migrations script 0 Initial -c DefaultDbContext --project src/Definition/EntityFramework/EntityFramework.csproj --startup-project src/Services/AdminService/AdminService.csproj --no-build --output "$env:TEMP\PerigonInitial.sql"` — passed; SQL 文件生成，26,106 bytes。
- [ ] TASK-003 [REQ-002] 完成 AdminService 集成构建验证
  - Depends on: TASK-001, TASK-002
  - Done when: `dotnet build src/Services/AdminService/AdminService.csproj -v q` 成功。
  - Verify: `dotnet build src/Services/AdminService/AdminService.csproj -v q` — blocked by duplicate `SysUserController` type/member definitions in untracked `SystemUserController.cs`.

## 实现记录

### `2026-09-29` — `TASK-001` / 模块化模型配置
- Status: `done`
- Implementation: 将资源定义 Restrict 配置放入 ResourceMod，将数据权限默认值放入 SystemMod；`DefaultDbContext` 通过 `ApplyConfigurationsFromAssembly` 自动注册程序集内全部 `IEntityTypeConfiguration`，后续新增配置类无需手动注册。PostgreSQL 原生支持数组，`TargetIds` 无需配置；SQL Server 无原生数组，其 JSON 默认值 `N'[]'` 在 `OnModelCreating` 中按 provider 设置。多对多使用显式关联实体。
- Code evidence: `src/Definition/EntityFramework/AppDbContext/DefaultDbContext.cs`; `src/Definition/EntityFramework/EntityConfigration/ResourceMod/UserResourceConfiguration.cs`; `src/Definition/EntityFramework/EntityConfigration/ResourceMod/UserResValueConfiguration.cs`; `src/Definition/EntityFramework/EntityConfigration/SystemMod/SysDataScopeGroupConfiguration.cs`
- Verification: `dotnet build src/Definition/EntityFramework/EntityFramework.csproj -v minimal` — passed; 重新生成 `Initial` 后 `TargetIds` 为 `uuid[]` 且无显式默认值，确认 PostgreSQL 依赖原生数组行为。
- Documentation: 本 PT、来源 PD、Demand/Design 索引已同步。
- Remaining: AdminService 集成构建单列为 TASK-003。

### `2026-09-29` — `TASK-002` / 重建迁移基线
- Status: `done`
- Implementation: 删除旧 migration history 和 snapshot，生成 PostgreSQL `Initial` 与新 snapshot。新迁移创建完整 schema，包括 `SysUserOrganizations`；`TargetIds` 为 `uuid[]` 且无显式默认值（依赖 PostgreSQL 原生数组行为）；未应用到数据库。
- Code evidence: `src/Definition/EntityFramework/Migrations/20260929094516_Initial.cs`; `src/Definition/EntityFramework/Migrations/DefaultDbContextModelSnapshot.cs`
- Verification: `dotnet ef migrations script 0 Initial -c DefaultDbContext --project src/Definition/EntityFramework/EntityFramework.csproj --startup-project src/Services/AdminService/AdminService.csproj --no-build --output "$env:TEMP\PerigonInitial.sql"` — passed; generated SQL file is 26,106 bytes.
- Documentation: PD records empty-database-only behavior and existing-data migration risk.
- Remaining: 未连接数据库，未实际执行 migration；SQL Server 迁移未生成。

### `2026-09-29` — `TASK-003` / AdminService 集成构建
- Status: `blocked`
- Implementation: 未修改重复控制器文件；保留其当前工作区状态。
- Code evidence: `src/Services/AdminService/Controllers/SystemMod/SystemUserController.cs`
- Verification: `dotnet build src/Services/AdminService/AdminService.csproj -v q` — failed with duplicate `SysUserController` type and duplicate members.
- Documentation: ProjectTracking 已记录 blocker。
- Remaining: 解决重复控制器后重跑 AdminService/solution build；在空 PostgreSQL 测试库可用后验证真实迁移。

## Progress
- Done: 2 / 3
- Blocked: TASK-003
- Next: 在不覆盖用户工作区文件的前提下解决重复控制器编译问题，然后重跑 AdminService build。
