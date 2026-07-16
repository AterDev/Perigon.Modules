# ResourceMod 开发实施说明

## 1. 目标与边界

`ResourceMod` 是面向租户的通用资源管理模块。资源由环境、分类、可选分组、标签和资源定义组成；资源定义驱动动态属性录入。模块在 `AdminService` 提供管理 API，并在 `src/ClientApp/WebApp` 提供验证用的后台界面。

首版只实现后台端到端能力，不为 `ApiService` 新增接口。Angular 代码是验证宿主的一部分，不进入模块 zip；模块包仅包含 `Entity/ResourceMod`、`Modules/ResourceMod`、`Controllers/ResourceMod` 和 `metadata.json`。迁移、AppHost 配置、测试和 Angular 源码均不进入包。

模块名称、程序集、命名空间、实体目录和控制器目录均使用 `ResourceMod`。目标服务为 `AdminService`；模块程序集必须提供公共静态 `ModuleExtensions.AddResourceMod(IHostApplicationBuilder)`，由现有源生成器发现和注册。

## 2. 数据模型与持久化

所有实体继承仓库当前适用的实体基类，受既有租户过滤和软删除约定约束。除特别说明外，所有唯一性均在同一 `TenantId` 内成立；外键和业务查询必须限制在当前租户，不能以客户端提交的 ID 跨租户关联数据。

| 实体 | 字段与约束 | 关系和删除规则 |
| --- | --- | --- |
| `ResEnvironment` | `Name` 必填，最大 60；`Icon` 可空，最大 60；`Color` 必填，最大 20。 | 一个环境有多个 `Resource` 和 `ResPermission`。仍有关联资源或授权记录时拒绝删除。 |
| `ResCategory` | `Name` 必填，最大 60；`Icon` 可空，最大 60；`Color` 必填，最大 20；`CatalogCode` 必填，最大 60。用户故事中的 `CatelogCode` 统一更正为 `CatalogCode`。 | `CatalogCode` 建立租户内唯一索引。一个分类有多个 `ResGroup`、`Resource` 和 `ResPermission`。仍有关联资源、分组或授权记录时拒绝删除。 |
| `ResGroup` | `Name` 必填，最大 60；`Description` 可空，最大 500；`Icon` 可空，最大 60；`Color` 必填，最大 20；`CategoryId` 必填。 | 多对一 `ResCategory`，一对多 `Resource`。仍有关联资源时拒绝删除。资源选择分组时，其 `CategoryId` 必须等于资源分类。 |
| `ResTag` | `Name` 必填，最大 60；`Color` 必填，最大 20；`Icon` 可空，最大 60。 | 仅是标签词表。资源持久化 `TagNames` 字符串数组，不建多对多表。标签改名或删除不回写已保存资源。 |
| `ResDefinition` | `Name` 必填，最大 60；`Icon` 可空，最大 60。 | 一对多 `ResDefinitionProperty`，一对多 `Resource`。存在资源时拒绝删除。 |
| `ResDefinitionProperty` | `Name` 必填，最大 60；`ValueType` 必填枚举；`IsRequired` 必填；`MaxLength` 必填，默认 200，范围 1–1000；`Sort` 必填。 | 多对一 `ResDefinition`，一对多 `ResValue`。同一资源定义内名称唯一；存在资源值时拒绝删除。属性变更只影响后续资源编辑校验。 |
| `Resource` | `EnvironmentId`、`CategoryId`、`DefinitionId` 必填；`GroupId` 可空；`TagNames` 必填，默认空数组。 | 资源本身不保存名称、图标 URL 或描述；展示名称和说明由 `ResDefinition` 及其属性定义提供。分别多对一环境、分类、定义和可选分组；一对多 `ResValue`。建立常用筛选索引：`EnvironmentId`、`CategoryId`、`GroupId`、`DefinitionId`。 |
| `ResValue` | `ResourceId`、`DefinitionPropertyId` 必填；`Value` 必填，最大 1000；`PropertyNameSnapshot` 必填，最大 60；`ValueTypeSnapshot` 必填枚举。 | 多对一资源和定义属性；同一资源的 `DefinitionPropertyId` 唯一。快照保证定义属性日后重命名或类型变更时，历史详情仍可正确显示。 |
| `ResPermission` | `RoleId`、`EnvironmentId`、`CategoryId` 均必填。 | `RoleId + EnvironmentId + CategoryId` 建立租户内唯一索引；角色 ID 指向 `SystemMod` 的 `SystemRole`，环境和分类为本模块外键。删除环境或分类前必须先清理授权。 |

`ValueType` 使用带 `Description` 的枚举：`String`、`Number`、`Boolean`、`Date`、`Uri`、`IPAddress`。`ResValue.Value` 始终存储字符串；其规范格式为：数字使用不受区域影响的十进制文本、布尔值为小写 `true`/`false`、日期为 ISO 8601 `yyyy-MM-dd`、URI 为绝对 URI 的规范文本、IP 地址为 `IPAddress.ToString()` 结果。

## 3. 后端模块与 API

### 3.1 模块切片

实现时创建实体、`DefaultDbContext` 的 `DbSet` 与必要模型配置、`ResourceMod` 项目、DTO、Manager、`AdminService/Controllers/ResourceMod` 控制器、`AdminService` 的项目引用、迁移、初始化和 API 测试。DTO 按实体放在 `Models/{Entity}Dtos` 下，至少提供 Add、Update、Detail、Item、Filter 形态；不要将实体直接作为列表和编辑契约。

控制器保持薄，只负责路由、绑定、授权和 HTTP 结果；Manager 负责租户过滤、关联校验、动态属性校验、事务和业务错误。预期业务失败抛出 `BusinessException`，由现有中间件返回 `ProblemDetails`，不引入 `ApiResponse` 包装。

### 3.2 API 契约

各基础实体采用仓库现有 REST 形态：`GET list`（分页和筛选）、`GET {id}`、`POST`、`PATCH {id}`、`DELETE {id}`。`ResDefinition` 详情必须携带按 `Sort` 排序的属性；`Resource` 详情必须携带环境、分类、分组、定义、标签名称和资源值快照。

除常规 CRUD 外，提供以下面向表单的只读选择接口，返回轻量 Item DTO，不分页或使用明确上限：

- 环境、分类、标签和资源定义选择列表；资源定义选择项带其有效属性。
- `GET groups?categoryId=`，只返回指定分类的分组；未选分类时不返回分组。
- `GET system-roles` 或等价的模块选择接口，返回当前租户可授权的 `SystemRole` 项；不得让前端通过用户列表推导角色。
- `GET permissions?environmentId=&categoryId=` 返回该组合的角色授权列表；保存授权使用以环境、分类和完整 `RoleIds` 为输入的替换式请求，事务内删除旧授权并写入去重后的新授权。

资源列表 `FilterDto` 支持名称关键字、`EnvironmentId`、`CategoryId`、`GroupId`、`DefinitionId`、标签名称和标准分页/排序。列表必须用投影和分页，不返回全部动态值；详情接口才返回完整 `ResValue` 集合。

### 3.3 授权与可见性

写入资源、基础数据、定义和授权配置仅允许 `_userContext.IsAdmin` 为真的后台管理员。管理员可以读取本租户全部资源。

非管理员只能调用资源列表与详情的读取接口，且只能看到满足以下条件的资源：当前用户在同一租户拥有至少一个 `SystemRole`，并存在 `ResPermission(RoleId, Resource.EnvironmentId, Resource.CategoryId)` 的精确匹配记录。角色 ID 必须从 `SystemUserRole` / `SystemRole` 关系取得；不能使用 `IUserContext.Roles` 中的角色名称替代 ID。列表查询在数据库层加入此条件，详情在按 ID 查询时加入相同条件；不匹配返回 403，不得仅依赖 Angular 隐藏按钮。

### 3.4 保存与删除规则

- 新建资源前，验证环境、分类和定义存在；`GroupId` 非空时验证分组存在且属于该分类；所有对象必须属于当前租户。
- 保存时以选定定义的当前属性集校验传入值：必填属性必须存在，禁止重复和未知属性，字符串遵守定义最大长度，所有值不得超过 1000；六种类型均按本节规范解析并重写为规范文本。失败返回 400 `ProblemDetails` 并指出字段。
- 定义或属性变化不批量改写历史 `ResValue`。编辑既有资源按当前定义重新校验并替换该资源的值集合；未在当前定义中的旧值保留，仅在读取历史详情时以快照展示。实现前需在 DTO 中显式区分“当前可编辑值”与“历史只读值”。
- 标签词表仅影响新建/编辑时的候选项；资源可保存多个去重后的标签名称，空数组合法。
- 删除环境、分类、分组、定义或定义属性遇到引用时返回 409 `ProblemDetails`；资源删除采用仓库既有软删除语义。删除标签不影响历史资源名称。
- 初始化逻辑必须幂等地为每个适用租户建立 `Dev` 环境和 `Default` 分类；重跑不得重复插入，也不得覆盖用户修改后的名称、颜色或图标。

## 4. Angular 验证端交互

所有页面使用 Angular 21 standalone 组件、严格类型、`OnPush`、signals 和 Material 组件。请求客户端以 AdminService Swagger 契约为准，通过 `scripts/NgRequestSync.ps1` 的既有流程生成后再封装页面调用；不手写服务 URL。路由、`src/assets/menus.json`、中英文 i18n 键和 `i18n-keys.ts` 必须同步。

### 4.1 导航与访问

新增两个菜单/路由入口：

- `resource`：资源列表、详情及编辑。
- `resource-config`：配置页，包含环境、分类、分组、标签、资源定义和资源权限六个标签页。

非管理员可进入资源只读页，且只展示服务端返回的数据；配置入口、新增、编辑、删除和内联创建控件对其隐藏并禁用。即使前端状态错误或手工构造请求，服务端仍必须返回 403。管理员路由与菜单的可见性使用登录用户的现有角色/菜单信息；不能以浏览器本地标记作为授权依据。

### 4.2 资源列表、详情和编辑

资源列表提供环境/分类/分组/定义下拉筛选、标签多选筛选、分页、刷新、创建和行操作。列表显示环境、分类、分组、定义和标签；新增、详情、编辑均使用 Material Dialog，不跳转页面。无数据时显示空态及管理员可用的创建入口。

资源详情显示基础信息、标签和动态属性（名称、类型、值），历史值使用 `PropertyNameSnapshot` 与 `ValueTypeSnapshot` 渲染。管理员可从列表或详情进入编辑；删除必须使用已有确认对话框。

新建和编辑使用一个弹窗分区表单，顺序和依赖如下：

1. 打开弹窗并行加载环境、分类、标签和资源定义；各列表非空时默认选择第一个环境、分类和定义。
2. 选定分类后加载该分类分组；切换分类时清空不属于新分类的分组。分组选择旁提供“新建分组”，保存成功后刷新列表并自动选中。
3. 标签采用多选；提供“新建标签”，保存成功后追加并自动选中。标签改名或删除不会修改已加载资源的标签名称。
4. 切换资源定义前提示用户动态属性将被重置；确认后用定义属性重新建立表单控件。编辑已有资源时，以当前定义属性预填匹配的值，并将不再属于当前定义的历史值单独只读显示。
5. 表单布局固定为：第一行环境和分类，第二行资源定义，第三行分组和标签，第四行开始动态属性两列排列。依属性类型呈现控件：字符串为文本框，数字为 number 输入，布尔为选择，日期为日期选择器，URI 和 IP 地址为文本框。前端即时执行必填、长度和类型校验；提交前禁止无效表单。API 返回字段级 `ProblemDetails` 时映射到对应控件并显示摘要。

### 4.3 配置页

- 环境、分类、分组、标签均为可筛选列表加新增/编辑对话框；分类编辑字段使用 `CatalogCode`。分组表单先选分类，图标和颜色控件与其他配置复用。
- 定义标签页显示定义列表和属性编辑器。属性以可增删、排序的表单数组展示，每行配置名称、类型、必填和最大长度；属性仍被资源值引用时禁用删除并提示后端原因。
- 权限标签页先选择环境和分类，再加载该组合的授权角色和系统角色候选项；使用多选角色控件，保存采用完整替换。页面展示当前组合、已授权角色数、加载/空态/保存反馈；切换环境或分类前未保存修改需确认。
- 图标输入使用 Material Icons 名称搜索与选择，保存名称而非 SVG；颜色提供灰、蓝、绿、黄、紫、橙、青、粉八个预设和原生颜色选择器。提交统一保存 CSS 颜色值，API 仍按最大 20 字符约束校验。

## 5. 验收与测试

| 场景 | 前置与操作 | 预期结果 |
| --- | --- | --- |
| 默认数据 | 新租户初始化后重复执行初始化。 | 仅存在一条 `Dev` 环境和一条 `Default` 分类；用户后续修改不被覆盖。 |
| 分类编码 | 在同一租户重复创建 `CatalogCode`。 | 第二次保存返回 409；其他租户可使用相同编码。 |
| 分组一致性 | 创建资源时提交属于另一分类的 `GroupId`。 | API 返回 400，资源不保存；前端切换分类后不会保留该分组。 |
| 动态属性 | 分别提交六种合法值，以及空必填值、超长字符串、非法数字/布尔/日期/URI/IP。 | 合法值按规范文本保存；非法值返回字段级 400，前端显示对应错误。 |
| 定义演进 | 创建资源后重命名属性或修改属性类型。 | 历史详情按快照显示；编辑按当前定义校验；定义/属性被引用时删除返回 409。 |
| 标签历史 | 创建带标签的资源后改名或删除该标签。 | 资源的 `TagNames` 不改变，按历史名称仍可筛选。 |
| 资源授权 | 为角色 A 授权环境 E、分类 C；角色 A 查询不同环境、不同分类及匹配资源。 | 仅匹配 E+C 的资源可见；列表在数据库过滤，详情越权返回 403。 |
| 管理员授权 | 管理员查询未配置 `ResPermission` 的资源并执行配置/资源写入。 | 可读取本租户全部资源并成功写入；普通用户访问同一写入接口返回 403。 |
| 引用删除 | 依次尝试删除被资源、分组、授权或资源值引用的对象。 | 返回 409，原数据完整保留；删除无引用对象成功。 |
| 前端流程 | 管理员完成资源新建、内联建分组/标签、定义切换、授权配置；普通用户访问只读资源页。 | 路由、菜单、i18n、生成客户端调用和页面反馈正确；普通用户无写控件且只看见 API 允许的数据。 |

后端行为通过 `test/ApiTest` 的 Aspire 集成路径验证，覆盖 HTTP 状态码、响应契约和持久化结果；测试数据库必须保持既有测试专用清理条件。Angular 改动在 `src/ClientApp/WebApp` 使用 `pnpm` 运行最小相关检查。实际开发时仅在数据库/API 改动已完成后生成迁移、请求客户端或模块包，并分别审查其差异；本实施文档本身不触发这些操作。
