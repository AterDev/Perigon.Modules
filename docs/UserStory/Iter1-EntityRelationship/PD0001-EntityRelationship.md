# Entity Relationship Mapping and Migration Baseline

## 文档信息
- Iteration: `Iter1-EntityRelationship`
- Product Design: `PD0001-EntityRelationship`
- Status: `in-progress`

## 背景与目标
`DefaultDbContext.OnModelCreating` 集中包含模块关系和字段默认值配置。模型应优先由导航属性、实体约定及显式关联实体表达；无法由约定或 Data Annotation 表达的规则应按模块独立配置。当前迁移历史需要重建为与现有模型一致的全新基线。

## 范围
### 包含
- 将必须保留的关系删除行为和 provider-specific 字段默认值拆入模块实体配置。
- 使用 `SysUserRole`、`SysMenuRole` 和 `SysUserOrganization` 表达关联关系。
- 删除历史迁移源码及快照，生成 PostgreSQL `Initial` 基线。

### 不包含
- 将新基线应用到已有数据库或执行数据库清库。
- 为 SQL Server 单独生成迁移。
- 迁移既有影子关联表中的生产数据。

## 角色与用户场景
本变更面向维护 Perigon 数据模型和数据库初始化流程的开发者，不改变终端用户的业务权限行为。

## 需求
### REQ-001 保留关系语义并模块化配置
- 优先依赖实体导航、显式外键及 EF Core 约定；只有无法通过约定表达的行为才需要额外模型配置。
- 优先级：P1
- 验收场景：SC-001
  - Given 使用 PostgreSQL 构建 `DefaultDbContext` 模型，When 模型配置完成，Then 资源定义关系保留 Restrict 删除行为，显式关联关系不生成重复的影子多对多表。

### REQ-002 从空迁移历史生成完整基线
- 基线迁移必须能从空数据库创建当前模型所需的完整 schema，包含显式关联表和索引。
- 优先级：P1
- 验收场景：SC-002
  - Given 数据库为空且迁移历史为空，When 生成并执行 `Initial` 迁移，Then 当前模型对应的 schema 可被创建。

## 非功能要求
- `SysDataScope.TargetIds` 在 PostgreSQL 中为原生 `uuid[]`，空数组是默认行为，无需配置；SQL Server 无原生数组，需 JSON 默认值。当前生成的基线目标为 PostgreSQL。
- 模型配置在 `EntityFramework` 项目内按 `EntityConfigration/{Module}` 组织。

## 边界与异常
- 全新 `Initial` 迁移是空数据库基线，不是已有数据库的增量升级脚本。已有数据库不可直接应用，否则创建已存在对象时会失败。
- 生成迁移不自动转换既有影子关联表的数据；已有环境需单独制定备份、数据迁移和切换方案。

## 假设与依赖
- 本次清理历史迁移是用户明确要求；旧迁移不再用于既有环境回滚。
- 本次根据 `appsettings.Development.json` 中的 provider 配置生成 PostgreSQL 迁移。

## 待确认项
- 若仍需支持已部署数据库，应在部署前确认既有数据保留与数据库基线接管方案；当前变更不执行该方案。

## 技术设计

### 上下文与约束
`DefaultDbContext` 使用 EF Core 10，支持 PostgreSQL 和 SQL Server。实体位于 Entity 项目，模型配置位于 EntityFramework 项目。多对多关系优先使用显式关联实体。

### 方案与需求映射
| Requirement | Design decision | Verification |
|---|---|---|
| REQ-001 | 非约定关系行为及字段默认值放入 `EntityConfigration/{Module}` 的配置类；关联关系由实体导航和显式 join entity 表达 | EntityFramework 构建、snapshot 检查 |
| REQ-002 | 删除旧 migration history 后生成完整 PostgreSQL `Initial` | 导出 `0 -> Initial` SQL 并检查创建内容 |

`DefaultDbContext` 在基类模型配置后通过 `ApplyConfigurationsFromAssembly` 自动注册程序集内全部 `IEntityTypeConfiguration`，后续新增配置类无需手动注册。所有配置类保持无参构造。PostgreSQL 原生支持数组，`SysDataScope.TargetIds` 无需配置；SQL Server 无原生数组，其 JSON 默认值 `N'[]'` 在 `OnModelCreating` 中按 provider 设置。

### 数据模型与迁移
新 PostgreSQL `Initial` 的 `Up` 从空历史创建当前完整 schema，包括显式 `SysUserOrganization` 关联表，不包含旧历史的 rename/drop；`Down` 仅回滚本迁移创建的对象。已有库需要独立的备份、影子关联数据转换和基线接管方案。

### 权限、租户与安全
不改变权限判定逻辑或租户过滤约定。关联实体继承 `EntityBase`，保留租户、软删除和审计字段。

### 兼容、发布与回滚
新基线仅适用于空数据库或明确重建的开发数据库。对已有库执行会因表已存在而失败，也没有自动的数据回滚能力。SQL Server 配置分支保留，但本次仅生成 PostgreSQL migration。

### 测试策略
- 构建 `EntityFramework.csproj` 验证配置类、migration 和 snapshot 编译。
- 导出从 `0` 到 `Initial` 的 SQL，验证 migration assembly 和 schema 脚本。
- AdminService 全量构建受工作区重复控制器阻塞；数据库连接不可用，因此本轮不执行真实 PostgreSQL 初始化。

### 备选方案
| Decision | Rationale | Alternatives |
|---|---|---|
| 使用模块级 `IEntityTypeConfiguration<T>` | 关系和默认值映射按模块归属，避免 DbContext 集中堆叠配置 | 全部留在 DbContext；约定无法表达 Restrict/provider-specific SQL |
| 多对多使用显式 join entity | 保留租户、软删除和审计属性并表达两个一对多关系 | 影子 join table 无法复用现有关联模型 |
| 重建单一 `Initial` | 用户明确要求清理历史迁移并按当前模型建立新基线 | 追加迁移兼容旧历史；不符合本次要求 |
