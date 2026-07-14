---
name: backend
description: Implement and review Perigon.Modules backend code on .NET 10, ASP.NET Core, EF Core, and Aspire. Use for entities, DbContexts, migrations, DTOs, Managers, Controllers, module DI, authentication, tenant-aware data access, background services, or backend performance and correctness reviews.
---

# Backend Development

Read the root `AGENTS.md`; for a new or packaged module also read `../module/SKILL.md`. Inspect the closest implementation and the exact base-class API before writing code.

## Follow the data flow

Implement a backend feature in this order:

1. Model the entity and relationships under `src/Definition/Entity/{Name}Mod`.
2. Add the required `DbSet` or model configuration in `EntityFramework`.
3. Define task-specific Add, Update, Detail, Item, and Filter DTOs under the module's `Models/{Entity}Dtos` directory. Do not create unused shapes mechanically.
4. Implement business logic under `Managers`, using the correct `ManagerBase` specialization and DbFactory.
5. Expose the feature through a thin Controller in the correct service.
6. Add initialization, worker, cache, or third-party services through the module only when required.
7. Add the focused verification described in `../test/SKILL.md`.

## Entities and EF Core

- Inherit `EntityBase` or the appropriate tenant-aware contract used by nearby entities.
- Use Guid identifiers according to the existing base implementation; do not replace ID generation ad hoc.
- Add `MaxLength` to strings, explicit precision to decimal values, indexes for real query/uniqueness requirements, and `[Description]` to enums.
- Prefer `DateTimeOffset`, `DateOnly`, and `TimeOnly` when their semantics fit.
- Model relationships with foreign keys rather than delimited strings. Use arrays/JSON only for intentionally value-like collections and verify provider support.
- Preserve soft-delete and tenant filters supplied by the shared context/Manager base.
- Generate migrations via `scripts/EFMigrations.ps1`; inspect the migration and snapshot. Do not edit historical migrations or run database updates unless requested.

## Managers and queries

- Keep C# readable: limit lines to 120 characters, split fluent queries and complex conditions at meaningful boundaries, and place one statement on each line. Use brace-delimited method bodies rather than expression-bodied methods (`=>`); this does not prohibit Lambda expressions used inside a method.
- Keep validation, business rules, transactions, and data access in Managers or module-owned domain services.
- Reuse `FindAsync`, `ExistAsync`, list/page, insert/update/delete, bulk, and transaction helpers only after checking their current signatures and permission semantics.
- Prefer projection to DTOs, pagination, and `AsNoTracking` for reads. Avoid N+1 queries, large unbounded results, and unnecessary `Include` graphs.
- Keep async flows asynchronous and pass `CancellationToken` where the called APIs and existing public contract support it.
- Throw `BusinessException` for expected business failures and let global middleware produce ProblemDetails.
- Avoid circular Manager dependencies. Existing Managers may coordinate related Managers for one use case; keep new coordination cohesive and transactional where needed.
- Put third-party integrations and reusable helpers in module services or `Share`, not Controllers.

## Controllers and HTTP contracts

- Inherit the repository's `RestControllerBase` form and inject the Manager through the primary constructor.
- Keep routing, binding, validation, authorization/permission checks, and HTTP result mapping in the Controller.
- Use explicit HTTP verb attributes and clear route parameters. Match neighboring API naming unless a public contract requires another name.
- Return domain/DTO results or `ActionResult<T>` as appropriate; use `CreatedAtAction`, `NotFound`, `Problem`, and correct status codes. Do not add an `ApiResponse` wrapper.
- Never access DbContext directly from a Controller.
- Treat authentication, tenant isolation, ownership checks, and destructive operations as required review points.

## Module services and initialization

Register module-owned services in `ModuleExtensions.Add{Name}Mod`. Keep host-wide framework registration in `ServiceDefaults`. Make initialization idempotent and avoid embedding production secrets or irreversible behavior. Background workers must honor cancellation and use scoped dependencies safely.

## Review and validate

Review correctness first: authorization, tenant leakage, nullability, uniqueness/races, transaction boundaries, query shape, error contract, migration impact, and module portability. Then run the smallest relevant compile/test path; use Aspire integration tests for database/API behavior rather than treating compile success as sufficient.
