---
name: native-aot
description: 评估和验证 MiniApi 的 Native AOT、Trim、Request Delegate Generator、System.Text.Json、依赖与发布兼容性。用于新增或修改 endpoint、DTO、序列化、反射、动态代码、第三方包、EF 模型或发布配置；普通文档和无运行时影响的改动不使用。
---

# MiniApi Native AOT

MiniApi 的 `ApiService.csproj` 默认启用 `IsAotCompatible`、`EnableRequestDelegateGenerator`、`PublishAot`，目标为 `linux-x64` 自包含发布。不能用普通 Debug build 代替 AOT 兼容证据。

## 先判断影响

以下变化视为 AOT-sensitive：

- Minimal API 路由、handler 参数/返回类型、endpoint filter 或 metadata；
- 请求/响应 DTO、JSON converter、多态、泛型或运行时类型；
- 反射、表达式编译、`dynamic`、程序集扫描、运行时生成或动态加载；
- DI 自动发现、Options 绑定、EF 模型、资源/本地化；
- 新增/升级第三方包或修改 publish/Docker 配置。

仅注释、文档、静态资源或能证明不进入发布闭包的变化可记录 `AOT impact: not-affected`，说明依据后免于 publish。

## 实现约束

- Endpoint group 继承 `RestEndpointBase`，提供 `public static void MapEndpoints(IEndpointRouteBuilder)`，由源生成的 `MapEndpointGroups()` 注册。
- handler 优先具名 `public static` typed method；参数绑定来源和返回契约保持静态可分析。不要用反射扫描路由或运行时生成 handler。
- 业务和数据访问放 Manager/Service，endpoint 只处理 HTTP 边界、授权、验证和结果塑形。
- 使用稳定、明确的请求/响应类型。需要定制或多态 JSON 时提供可静态生成的 `JsonSerializerContext`/resolver，并在发布产物中验证真实序列化。
- DTO 与实体的转换遵循 `perigon` backend reference：由 AI 在对应 DTO/实体类型中编写显式方法；不引入对象映射包、映射代码生成器/工具或独立映射扩展。
- 引入反射或动态 API 前先检查对应 API 和依赖的 AOT/Trim 标注。优先源生成、显式注册和静态映射；不要靠随意添加 linker descriptor、`DynamicDependency` 或 warning suppression 掩盖未知可达性。
- 对确有依据的保留或抑制，记录目标成员、原因、可达性假设和回归验证。

## 验证阶梯

在 `MiniApi` 根目录按影响递进执行：

```powershell
dotnet build MyProjectName.slnx -c Release
dotnet test --project tests/ApiTest/ApiTest.csproj
dotnet publish src/Services/ApiService/ApiService.csproj -c Release -r linux-x64 --self-contained true -p:PublishAot=true -p:PublishTrimmed=true
```

- 审查完整输出中的 linker/AOT/RDG warning，尤其是包含 `IL2`、`IL3` 或 endpoint source-generation 诊断的警告；不只看退出码。
- AOT-sensitive 变化的 publish 成功后，还要运行发布二进制或目标容器，检查 `/health`、`/alive` 和受影响 endpoint 的绑定、JSON、鉴权及错误响应。
- Docker 发布使用 `scripts/PublishDocker.ps1`；运行前核对脚本和 Dockerfile 不是空占位，并确认本机具备 Docker/Podman 与目标架构能力。
- 若仓库基线自身导致 publish 失败，保留首个根因和精确命令，在 PT 标记 blocked；不能把普通 build 通过写成 AOT 通过。

## 完成证据

记录变更类别、AOT 影响判断、精确命令、publish/runtime 结果、warning 处置和未验证目标架构。只有 AOT-sensitive 变化通过所需 publish 与运行验证，或有明确批准的 waiver，才能作为完成证据。
