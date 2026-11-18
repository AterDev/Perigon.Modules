# MiniApi 后端开发

## 实体与 DbContext

- 实体通常继承 `EntityBase`，明确字符串长度、decimal 精度、唯一性、软删除和时间语义。
- `EntityFramework.AppDbContext.DefaultDbContext` 接收 `Perigon.PostgreSQL.Options.DbContextOptions<DefaultDbContext>`，由 Perigon.PostgreSQL 的 `AddDbContextPool` 注册；连接字符串从 `builder.Configuration` 获取。池化实例的连接配置必须稳定，不可跨 scope 使用；增加请求或租户等可变状态时必须在每次租用后赋值并在 `OnPoolReturn` 中清除。不要使用 EF Core 的上下文 API，也不要虚构 ApiStandard 的多数据库 factory 或租户上下文。
- 使用 Perigon.PostgreSQL 的注册选项 `EnableOpenTelemetry` 接入 Npgsql 追踪与指标，并按部署需要配置连接池及连接、命令超时；它不负责配置 OpenTelemetry 导出器。
- 实体索引及其他可用映射优先使用 Perigon.PostgreSQL 特性；默认表名由约定生成，无需标记表名。仅在约定不满足需求时显式配置表名。
- 不要在 `OnModelCreating` 重复配置实体特性或默认约定已经表达的表名、索引等内容；特殊映射才使用 Fluent 配置。
- 新增实体后在相应 `DefaultDbContext` 添加显式 `DbSet<TEntity>`，保留 `DynamicallyAccessedMembers` 约束并执行 AOT publish 验证。
- MiniApi 不内置 AppHost migration 资源。数据库 schema 的生成、部署顺序、失败恢复和回滚责任必须在计划与部署管线中明确。

## 请求/响应模型

- Endpoint 所用请求和响应 DTO 按对应实体放在所属业务模块的 `Models/<Entity>Dtos` 目录，每个类型单独一个文件；不要在 Endpoint 类中定义 DTO，也不要使用 `Request` 或 `Response` 作为类名后缀。不要把数据库实体直接作为 HTTP 契约。
- DTO 与实体的转换使用显式 C# 方法，写在对应的 DTO/实体类型中；不使用 Mapster、AutoMapper 等映射包、映射代码生成器或独立映射扩展。DTO 可提供 `FromEntity`、`ToEntity` 或 `ApplyTo`，逐项处理属性；实体侧只承载自身的创建和领域更新逻辑。遵守依赖方向：`Definition/Entity` 不得引用 `Modules` 中的 DTO；在 DTO 中调用实体方法时，保留实体不变量并明确允许写入的字段。
- Add/Update 排除 Id、系统时间、软删除和不可赋值字段；Update 可空性准确表达更新语义。
- 列表/详情避免泄漏导航、无界集合、长文本、二进制或敏感字段。
- Filter 只暴露有明确语义和可控成本的筛选；所有列表必须有边界或分页。
- 新增或改变 JSON 契约时检查 AOT/Trim 与真实 HTTP 序列化。需要 converter、多态或运行时类型时使用可静态生成的 System.Text.Json 元数据策略。
- 开发版通过 `perigon aot jsonContext` 显式更新各服务的 `ApiJsonSerializerContext`；接口和模块模型变化后重新运行，并检查生成结果与 Native AOT 序列化。保留手写声明，运行时多态和无法静态推断的类型仍需手工添加。Request Delegate Generator 不生成 JSON 元数据；CLI 命令尚待发布。

## Manager 与 Service

- 数据访问和业务流程放 Manager；第三方或基础设施依赖放可注入 Service。Endpoint 不直接堆积 DbContext 查询与事务。
- CRUD Manager 按需要继承 `ManagerBase<TDbContext,TEntity>`；无实体时使用非泛型基类。
- 非抽象、非泛型 Manager 由源码生成器注册，不要重复手写 DI 注册。
- Manager 返回领域结果或响应模型，不返回 `IResult`，不依赖 `HttpContext`。
- 优先复用分页、CRUD、批量和事务能力。表达式必须保持数据库可翻译和 AOT 可分析；不要用运行时编译表达式替代查询。
- 工厂或手动创建的上下文及时释放；事务范围小而明确。
- 业务异常使用项目既有 `BusinessException`/全局中间件策略，不吞掉错误。

## Minimal API Endpoint

- Endpoint group 继承 `RestEndpointBase`，实现 `public static void MapEndpoints(IEndpointRouteBuilder endpoints)`；源码生成器会生成 `MapEndpointGroups()` 调用。
- Endpoint 文件和类型优先以对应实体名称为前缀，例如 `UserEndpoints`；新增实体接口时，路由组和 OpenAPI tag 也使用对应实体名称。修改已有路由属于公开契约变更，须同步调用方、文档及验证。
- 使用 `MapGroup` 统一路径、tag 和授权策略。路由、HTTP 谓词和 OperationId/metadata 应稳定且唯一。
- 为公开接口提供准确的请求/响应元数据及适当的摘要、描述和 operationId，并验证生成的 OpenAPI JSON 与运行时行为一致。
- 返回 JSON 成功 DTO 与统一错误的接口可使用 `Task<ApiResult<TDto>>`：直接返回 DTO、常见 `TypedResults` 或基类 helper（如 `NotFound(Localizer.NotFoundUser)`、`BadRequest(Localizer.InvalidUserOrPassword)`）。helper 直接将资源键放在 detail，统一 writer 按当前请求语言尝试本地化所有非空 detail，找不到资源时保留原文，无需向 handler 注入 `Localizer`；`Problem` 默认使用 500。`Forbid` 返回带 Problem Details 的 403，不执行认证方案的 `ForbidAsync`。`ErrorResult` 继承 `HttpValidationProblemDetails`，包含 traceId 和字段错误 `errors`（普通错误为空）。`ApiResult<TDto>` 自动描述 200 的 DTO 和 400 的 `ErrorResult`；其他状态码通过 `Produces` 补充。任意 `IResult` 可通过 `ApiResult<TDto>.FromResult(...)` 包装。
- .NET 10 自动验证在 Endpoint 所在程序集及声明 DTO 的业务模块内分别调用 `AddValidation`；跨程序集 DTO 作为验证根需标记 `[ValidatableType]` 并声明数据验证特性。该标记在 .NET 10 是实验性 API；检查实际源生成结果并通过真实 HTTP 与 AOT 验证，不能只依赖成功构建。
- 使用基类 helper 返回本地化错误；`TypedResults.Problem` 的 detail 也会尝试本地化，未匹配的内容保留原样。`UseStatusCodePages` 为尚无响应体的错误状态补充统一格式。`TypedResults.BadRequest(value)` 已有响应体，不会被自动转换为 `ErrorResult`。
- 全局异常经统一 Problem Details writer 输出 `ErrorResult`；即使 Accept 不包含 JSON，错误也返回 `application/problem+json`。生产环境的意外异常只返回通用提示并记录日志；响应已开始或请求取消时不得尝试再次写响应。
- Handler 优先具名 `public static` typed method，显式表达路由、查询、header、body 和 DI 绑定；避免反射驱动绑定、运行时扫描或无法由 Request Delegate Generator 分析的动态行为。
- Endpoint 只负责 HTTP 边界、输入验证、授权、调用 Manager/Service、状态码和响应塑形。
- 使用标准状态码和项目现有错误模型。认证、授权、not-found、conflict 和验证失败路径均应有契约与测试。
- 公开契约变化同步 OpenAPI 和客户端，并通过实际 HTTP JSON 路径测试；不能只直接调用 handler。

## 横切能力

- 共享能力放 `Definition/Share` 或 `ServiceDefaults`，服务专属能力留在 ApiService；不要为少量代码增加新层。
- 缓存键包含必要的数据隔离维度，值大小与过期策略按变化频率设置。
- 日志使用 `ILogger` 与 OpenTelemetry，不记录 token、密码或敏感正文。
- JWT/OAuth 生产配置通过 Secret/环境变量注入；生产 OAuth/OIDC 要求 HTTPS metadata。
- 反射、动态代码、程序集扫描、通用 JSON helper 和新第三方包都是 AOT-sensitive；实现和审查时使用 `native-aot`。

## 数据库变更

先修改实体、`DbSet` 和映射，再使用项目选定的迁移方式生成并审查 schema 变化。不要手写已生成迁移，也不要修改已进入生产的历史迁移。部署前明确迁移执行者、备份/恢复点、前后版本兼容窗口和 API 启动顺序。
