# PostgreSQL 模型、租户与迁移基线

## 文档信息

- Iteration: `Iter1-EntityRelationship`
- Product Design: `PD0001-EntityRelationship`
- Status: `done`
- PostgreSQL provider: `Perigon.PostgreSQL 0.2.2`

## 背景与目标

模块项目统一使用 `Perigon.PostgreSQL 0.2.2`，移除 EF Core 的运行时依赖，同时保持现有模块目录、实体表名、列名、租户隔离和业务行为。`EntityFramework` 目录名称暂时保留用于兼容现有项目引用，但其中的上下文和查询实现不使用 EF Core。

本轮只支持 PostgreSQL，不再维护 SQL Server 配置、迁移或兼容分支。

## 范围

### 包含

- 使用 Perigon.PostgreSQL 上下文、显式查询、插入和更新操作。
- 保留租户过滤和软删除过滤；租户级 Manager 必须绑定有效 `TenantId`。
- 多对多关系只通过显式中间实体和中间表实现，不依赖隐式多对多表。
- 生成 PostgreSQL 初始迁移，并提供显式迁移、更新和 baseline 脚本。
- 保留旧 EF 数据库的表名、列名、索引和外键元数据，供基线校验使用。

### 不包含

- SQL Server provider、SQL Server 迁移或 SQL Server 兼容处理。
- 自动将既有数据库标记为已迁移。
- `EnsureCreated`、删除表、清库或自动重建已有数据库。
- 追踪机制和 `SaveChanges` 工作流。

## 需求

### REQ-001 PostgreSQL-only 数据访问

- 所有模块使用 `Perigon.PostgreSQL 0.2.2`。
- Resource 搜索的定义名称、资源值和标签元素模糊匹配由 PostgreSQL 执行；用户收藏先按当前用户可见资源过滤，再由数据库执行计数、排序和 `Skip/Take` 分页。
- `scripts/Migrations.ps1` 调用 `dotnet perigon migrations`，输出固定到 `PostgresMigrations`；兼容入口不得调用 `dotnet ef`。
- 优先使用显式 `Where`、`ToListAsync`、`FirstOrDefaultAsync`、`InsertAsync` 和 `ExecuteUpdateAsync`。

### REQ-002 租户和软删除安全

- 租户实体默认应用租户过滤和软删除过滤。
- 没有 `TenantId` 的请求不得创建租户级 Manager。
- 匿名接口只能解析不需要租户的 catalog Manager；租户级 Manager 必须延迟到认证和租户上下文建立后再解析。
- 测试环境使用临时 PostgreSQL 容器，非测试环境保留数据卷，避免测试复用开发库。

### REQ-003 显式关联表

- `SysUserRole`、`SysMenuRole`、`SysUserOrganization` 等关联必须使用显式实体和实体表。
- 外键删除行为、约束名称和关联列必须与既有 schema 核对后再生成或执行迁移。

### REQ-004 枚举整数持久化

- Perigon.PostgreSQL 0.2.2 的数据库列使用整数属性，不依赖枚举值转换。
- `Article.LanguageTypeValue` 映射到既有 `LanguageType` 列；`Article.ContentTypeValue` 映射到既有 `ContentType` 列。
- `LanguageType` 和 `ContentType` 仅作为 `[NotMapped]` 枚举包装属性，整数 backing 属性使用 `[JsonIgnore]`，避免改变 API JSON 结构。
- 服务端筛选和更新使用整数属性；枚举只在业务边界转换。

### REQ-005 迁移与旧库安全

- 空数据库可以执行初始 PostgreSQL migration 创建 schema。
- 旧 EF 数据库在 baseline 前必须备份并核对表、列、主键、索引、外键和删除行为。
- baseline 必须使用明确 migration id；系统不能自动 baseline，也不能因 schema 不一致而删除或重建对象。
- 每个独立数据库连接都必须单独执行迁移或 baseline，不能跨租户数据库共享迁移历史。

## 技术设计

### 上下文与目录

`src/Definition/EntityFramework` 是历史兼容目录，实际基类为 Perigon.PostgreSQL context。`DefaultDbContext` 负责连接、租户上下文和迁移模型；`AppDbFactory` 根据 catalog 或租户连接创建上下文。

### 迁移流程

- 添加/查看迁移：`scripts/Migrations.ps1`。
- 更新新数据库：`scripts/DatabaseUpdate.ps1 -Connection <connection>`。
- 接管旧库：先人工验证 schema，再显式执行 `scripts/DatabaseUpdate.ps1 -Baseline -Migration <id>`。
- `AdminService-Migrations` 是 AppHost 中的一次性 migration resource；其成功完成后，业务服务才启动。

### 兼容和发布

模块修改涉及 `Entity`、`Modules` 或对应控制器时，CMSMod、ResourceMod、SystemMod 均需重新打包。模块版本和包元数据以 `modules.json` 为账本，正式打包前不得使用 CLI 默认版本覆盖已有版本。

## 当前验证状态

- Entity、三个模块、AdminService 和 AppHost 的构建命令已分别执行。
- 依赖升级后的完整业务测试 `26/26` 已通过，覆盖资源搜索、收藏分页、跨租户访问和软删除隔离。
- 旧 EF 数据库 baseline 未自动执行；实际接管前仍必须备份并人工核对 schema、索引、外键和删除行为。
