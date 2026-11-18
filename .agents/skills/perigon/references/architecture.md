# MiniApi 架构、目录与配置

## 架构取向

MiniApi 面向轻量 Minimal API 和 Native AOT：优先静态可分析、显式、低开销的实现，减少层级与运行时发现。不要把 ApiStandard 的 Controller、AdminService、多数据库或 EF migration resource 套入本模板。

## 分层职责

- `src/Services/ApiService/Endpoints`：HTTP 路由、绑定、授权、验证和响应。
- `src/Modules/<Module>/Managers`：业务流程与数据访问；由源码生成器自动注册。
- `src/Modules/<Module>/Models/<Entity>Dtos`：对应实体的接口 DTO。
- `src/Modules/<Module>/Services`：模块内需要 DI 的服务实现。
- `Definition/Entity`：核心实体。
- `Definition/EntityFramework`：PostgreSQL DbContext、映射和 schema 模型。
- `Definition/Share`：跨层共享的常量、模型、基类和服务。
- `Definition/ServiceDefaults`：Aspire、框架服务和 Web middleware 注册。
- `Perigon.AspNetCore.SourceGeneration` NuGet 包：编译期生成 Manager、module compatibility hook 和 Endpoint group 注册。
- `AppHost`：PostgreSQL、可选 Redis 与 ApiService 编排，不承载业务逻辑。

依赖由定义流向实现再流向服务。不要把 `HttpContext` 或宿主实现泄漏到 Manager 和可复用服务。

## Endpoint 与源生成

- Endpoint 类继承 `RestEndpointBase`，声明 `public static MapEndpoints(IEndpointRouteBuilder)`。
- `Program.cs` 调用生成的 `AddManagers()`、`AddModules()` 和 `MapEndpointGroups()`；除非源生成器无法表达需求，不重复创建反射扫描或手工注册体系。
- `EnableRequestDelegateGenerator=true`，handler 必须保持 typed、静态可分析；路由与 OpenAPI metadata 应明确稳定。

## Native AOT 边界

`ApiService.csproj` 默认启用 `IsAotCompatible`、`PublishAot`、自包含 `linux-x64` 和符号裁剪。新增 endpoint/DTO、JSON converter、多态、反射、表达式、DI 扫描、EF 模型或第三方包时使用 `native-aot`，不能只验证 JIT build。

## ServiceDefaults

- `AddServiceDefaults`：Aspire、服务发现、重试、健康检查、OpenTelemetry。
- `AddFrameworkServices`：Options、PostgreSQL DbContext、缓存和框架依赖。
- `AddMiddlewareServices` / `UseMiddlewareServices`：认证授权、CORS、本地化、OpenAPI、限流和 Web 管线。

所有服务共同需要的行为放 ServiceDefaults；ApiService 专属配置和实现保留在服务中。生成的 Manager/Endpoint 注册不要重复手写。

## 配置边界

- AppHost `appsettings*.json` 选择开发基础设施并注入连接字符串和组件设置。
- ApiService `appsettings*.json` 管理认证、CORS、缓存策略和服务配置。
- 环境变量 `__` 对应配置路径 `:`。
- `Components:Cache` 为 Redis/Hybrid 时才需要 Redis 资源；Memory 模式不无故启动 Redis。
- 直接启动 ApiService 时，必须自行提供 AppHost 原本注入的连接与组件配置。
- 密钥、JWT Sign 和第三方凭据只从环境变量、Secret Store 或密钥服务注入。

以当前 Options 类型、AppHost 和实际 publish 结果验证配置，不创造不存在的开关。
