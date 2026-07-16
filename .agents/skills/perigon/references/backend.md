# Backend Reference

This reference belongs to the Perigon skill and covers the Perigon backend architecture used by this template.

## When to use

- Design entities, managers, DTOs, controllers, and service defaults.
- Wire business logic, migrations, and service-layer structure.
- Implement or review backend logic in a Perigon-based solution.

## Core rules

- Keep business logic in managers instead of controllers.
- Prefer `ManagerBase<T>` / `ManagerBase` and `RestControllerBase` patterns.
- Keep controller layers focused on input validation, authorization, and HTTP response shaping.
- Use shared utilities from `src/Perigon` before adding local duplicates.
- Use Aspire and service defaults where relevant to the runtime model.
- Avoid direct `DbContext` access in controllers; prefer manager or factory-based flows.
- 使用文件作用域命名空间、主构造函数、集合表达式和可空引用类型。
- Controller 只处理 HTTP 边界、权限和输入验证；业务逻辑放在 Manager；不要在 Controller 中直接访问 DbContext。
- 业务验证错误使用 `BusinessException`；API 错误优先使用 `Problem()` 或 `NotFound()`。
- 异步方法优先传递 `CancellationToken`，避免同步阻塞和不必要的内存分配。
- 优先复用 `src/Perigon` 提供的扩展和工具，例如 `Merge`、`MapTo`；多语言文本使用 `Localizer` 常量。
- 修改实体或 DbContext 后，通过 `scripts/EFMigrations.ps1` 生成迁移；不要手动编写或修改迁移文件。
- 优先保持一个类型一个文件；对于小型枚举，可以在同一文件中定义；对于实体和 DTO，优先使用 class 而非 record。
- 测试代码可适当使用 `record` 来简化数据结构定义，提升可读性和维护性。

## DTO 目录与命名规范（强制）

- 模块 DTO 必须放在 `Models/{Entity}Dtos` 下，`{Entity}` 使用对应实体名称；例如资源相关 DTO 放在 `Models/ResourceDtos`，资源定义相关 DTO 放在 `Models/ResDefinitionDtos`。
- 每个文件只能包含一个 DTO 类型。禁止使用 `Contracts.cs`、`Dtos.cs` 或其他聚合文件承载多个 DTO。
- 数据转换类型统一以 `Dto` 结尾，禁止使用 `Input`、`Request`、`Response` 命名 DTO。按用途使用 `{Entity}AddDto`、`{Entity}UpdateDto`、`{Entity}FilterDto`、`{Entity}DetailDto`、`{Entity}ItemDto` 等明确名称。
- DTO 的嵌套成员必须继续使用 DTO 类型；不要以实体类或 `Input`/`Request`/`Response` 类型代替 DTO。
- 新增、重命名或生成 DTO 后，必须同步更新 Manager、Controller、测试和客户端生成入口，并用搜索确认旧命名没有残留，再执行受影响项目的构建或测试。

## Development flow

1. Define entities, DbContext, and shared services.
2. Implement modules and managers / DTOs.
3. Implement controllers and API endpoints.
4. Run build validation (`dotnet build`) for the affected project or solution.
5. If entities are changed, review the existing migration workflow and scripts instead of manually altering migrations.

## Verification

- Run `dotnet build` after backend changes.
- If the change touches the domain model, review migration and startup behavior before concluding.
