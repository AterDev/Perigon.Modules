# Module Development Reference

Read `../../../../AGENTS.md` and the relevant user story. Use `CMSMod` for a small CRUD example and `SystemMod` for initialization, workers, services, and complex module behavior.

## Define the module contract

Before generating code, confirm:

- the `{Name}Mod` name and user-facing purpose;
- whether endpoints belong to `AdminService`, `ApiService`, or both;
- entities and relationships owned by the module;
- required permissions, tenant behavior, initialization, workers, and external dependencies;
- whether UI is only for repository validation or is expected to be distributed separately.

Keep the portable boundary explicit. Current zip packages contain `Entity/{Name}Mod`, `Modules/{Name}Mod`, `Controllers/{Name}Mod`, and `metadata.json`; they do not contain host configuration, migrations, Angular sources, or shared Perigon framework code.

## Create and wire a module

1. Prefer the currently available Perigon CLI or MCP generators. Inspect live help/tool schemas first; never invent a command or tool name from old documentation.
2. Require the module assembly name to end in `Mod`.
3. Add entities under `src/Definition/Entity/{Name}Mod` and register them in the appropriate DbContext.
4. Add `src/Modules/{Name}Mod/{Name}Mod.csproj`, DTOs, Managers, and optional services/workers/initialization.
5. Add a public static `ModuleExtensions` with public `Add{Name}Mod(IHostApplicationBuilder)` matching the assembly name exactly. Put module-owned DI in that method.
6. Apply `[DisplayName("Perigon::{Name}Mod")]` and `[Description("...")]` consistently because packaging derives module identity from the module assembly.
7. Reference the module project from every target service. The source generator discovers referenced `*Mod` assemblies, generates `AddModules()`, and registers `ManagerBase` implementations.
8. Add Controllers under the target service's `Controllers/{Name}Mod` directory.
9. Add migrations, initialization, tests, and optional host UI only when required by the contract.
10. Add the project to `Perigon.Modules.slnx` and verify there are no stale paths.

Do not duplicate generated `AddModules()` or manager registration manually. If discovery fails, inspect `ManagerSourceGen.cs`, the service's analyzer reference, the project reference, assembly name, `ModuleExtensions` accessibility, and `Add{AssemblyName}` signature.

## Generate and review code

Use generator output as a starting point. After generation, review namespaces, nullable annotations, DTO shapes, authorization, tenant boundaries, query size, service ownership, and package portability. Do not let a host-only dependency leak into the module project.

When adding a dependency, decide whether it is available to consumers of the module. Prefer existing `Definition/Share` contracts; only reference `Perigon.AspNetCore.Toolkit` when the module genuinely needs it.

## Package

For a single module, the repository's current contract is:

```powershell
perigon module pack <ModuleName> AdminService
```

For all `*Mod` directories, use `scripts/PackModules.ps1`. Packaging mutates `package_modules` and writes a metadata summary according to the script's current configured path, so inspect the script and working tree before running it.

After packaging:

1. Open the produced zip and verify `metadata.json` plus the expected Entity, Modules, and Controllers paths.
2. Check `ModuleName`, author, display name, description, version, package type, and service mode.
3. Ensure unrelated host or framework files were not included.
4. Compare the package and catalog diff; do not hand-edit zip contents.

Do not package for an instructions-only or source-only task unless release artifacts are explicitly in scope.

## Validate

- For structural changes, inspect project references, source-generator conventions, solution entries, and package paths.
- For backend behavior, follow `backend.md` and add focused tests from the repository's test skill.
- For migrations, run repository scripts from their expected working directory only after confirming database configuration.
- Treat CLI/container/runtime failures separately from module regressions and retain the original diagnostic.
