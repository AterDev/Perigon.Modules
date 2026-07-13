---
name: engineer
description: Route and coordinate repository-wide engineering work in Perigon.Modules. Use for new official modules, cross-layer changes, architecture or code review, ambiguous repository tasks, and work spanning module packaging, .NET backends, Angular, Aspire, or tests.
---

# Engineer

Read the root `AGENTS.md` first. Treat this skill as the task router and integration checklist, not as a replacement for the specialized skills.

## Route the task

- For a new module, module metadata, integration wiring, or packaging, read `../module/SKILL.md`.
- For entities, EF Core, DTOs, Managers, Controllers, services, or migrations, read `../backend/SKILL.md`.
- For work under `src/ClientApp/WebApp`, read `../angular/SKILL.md`.
- For unit tests, API tests, Aspire test hosting, or regression verification, read `../test/SKILL.md`.
- For reviews, load the skill for every layer touched and report correctness, compatibility, security, data, and packaging findings before style suggestions.

Do not load all skills for a narrow task. A complete new backend module normally needs `module`, `backend`, and `test`; add `angular` only when the requested module includes host UI work.

## Work from evidence

1. Read the relevant user story or development note.
2. Inspect the closest existing implementation and the source generator or script that owns implicit behavior.
3. Define the affected package boundary and target service before editing.
4. Trace the full slice: entity and persistence, module assembly, service endpoint, optional UI, tests, then package metadata.
5. Preserve unrelated working-tree changes and avoid expanding into synced framework code.

When requirements are incomplete, implement only the parts supported by repository evidence. Ask for a decision only when a choice changes the public contract, data model, permissions, or package boundary.

## Integrate consistently

- Keep names aligned across the `{Name}Mod` assembly, namespaces, folders, `ModuleExtensions`, service references, controllers, and package metadata.
- Prefer existing base types and generated registration over parallel abstractions.
- Keep infrastructure and host changes separate from portable module source.
- Do not assume the Angular verification client belongs in a module zip.
- Update documentation only when behavior or the developer workflow changes.

## Validate proportionally

Review the diff first. Run the smallest check that can fail for the changed layer, and preserve the primary error when the environment prevents validation. Never run migrations, mirror synchronization, AppHost, or package generation merely to validate an instructions-only change.
