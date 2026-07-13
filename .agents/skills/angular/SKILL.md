---
name: angular
description: Implement and review the Angular 21 verification client in src/ClientApp/WebApp. Use for standalone components, routes, Material UI, signals, forms, generated API clients, authentication, menus, i18n, theming, accessibility, or frontend tests related to Perigon modules.
---

# Angular Development

Work only under `src/ClientApp/WebApp` unless the task explicitly includes backend contracts or packaging. This app validates and demonstrates modules; do not assume its files are included in module zip packages.

## Use the existing layout

- Keep bootstrap/configuration in `main.ts`, `app.config.ts`, and `app.routes.ts`.
- Place route pages under `app/pages`, shell/navigation under `app/layout`, shared UI under `app/share/components`, and pipes under `app/share/pipe`.
- Keep API clients and models under `app/services`; generated admin clients live below `app/services/admin`.
- Preserve `customer-http.interceptor`, `auth.service`, `auth.guard`, environment files, and `proxy.conf.json` behavior.
- Maintain navigation metadata in `src/assets/menus.json` and translations in `src/assets/i18n/*.json`; keep keys aligned with `app/share/i18n-keys.ts` and the `i18n:keys` script.

## Angular and TypeScript conventions

- Use strict TypeScript, inference for obvious types, and `unknown` instead of `any` when the type is uncertain.
- Use standalone components; Angular 21 makes standalone the default, so do not add `standalone: true`.
- Prefer `inject()`, signals, `computed()`, `input()`, and `output()`; update signals with `set`/`update`, never `mutate`.
- Use `ChangeDetectionStrategy.OnPush` for components with stateful UI.
- Use native template control flow (`@if`, `@for`, `@switch`), class/style bindings instead of `ngClass`/`ngStyle`, and keep template expressions simple.
- Prefer typed reactive or signal forms with visible validation messages.
- Use async pipe for observable rendering. For necessary manual subscriptions, use `takeUntilDestroyed`.
- Put host bindings/listeners in the decorator `host` object instead of `@HostBinding`/`@HostListener`.
- Use `NgOptimizedImage` for static non-base64 images.
- Keep feature routes lazy-loaded and styles colocated. Use Angular Material plus `styles.scss`, `theme.scss`, and `vars.scss`; avoid global overrides unless the theme requires them.
- Preserve keyboard access, labels, focus behavior, ARIA semantics, and Material accessibility patterns.

## Integrate APIs

Confirm the backend Swagger contract before editing calls. Prefer regenerating request clients through the repository's Perigon flow (`scripts/NgRequestSync.ps1`) when the API contract changes, then review the generated diff. Do not hardcode service URLs or silently hand-edit many generated client files.

Update route, menu, permission/access code, translation, and page behavior as one coherent change. Reuse existing dialogs, paginator, avatar, tag, layout, and service patterns before adding new abstractions.

## Validate

Use `pnpm` in `src/ClientApp/WebApp`. Run the smallest relevant script; `start` and `build` also regenerate i18n keys. Do not start the Angular dev server or regenerate clients for an unrelated docs/backend task. Separate frontend compiler failures from an unavailable backend or proxy target.
