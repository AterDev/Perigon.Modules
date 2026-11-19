# Angular Reference

适用于 Perigon Modules 和 MiniApi 的 Angular 22 前端。先检查工作区的 `package.json` 和 `angular.json`，确认模板包含 Angular 客户端后再执行相关步骤。

## When to use

- Build CRUD pages, forms, routes, shared components, and Material-based layouts.
- Generate typed request clients from backend OpenAPI through Perigon.
- Implement or review frontend changes in a Perigon-based project.
- When editing Angular files: component, template, route, form, Material UI, signals, i18n, generated request client

## Project structure

```sh
src/
  ├── main.ts
  ├── app/
  │   ├── app.config.ts
  │   ├── app.routes.ts
  │   ├── layout/
  │   ├── pages/
  │   ├── modules/
  │   │   ├── share/          # 打包的基础依赖，供所有业务模块复用
  │   │   │   ├── components/
  │   │   │   ├── pipe/
  │   │   │   ├── auth.guard.ts
  │   │   │   ├── custom-paginator-intl.ts
  │   │   │   └── i18n-keys.ts
  │   │   └── {module}/       # 业务前端模块
  │   └── services/
  ├── assets/i18n/
  ├── environments/
  ├── styles/
  └── proxy.conf.json
```

## Core rules

- 组件默认standalone模式，默认拆分成html/scss/ts文件.
- UI 优先使用 Angular Material；样式优先复用现有样式`styles.scss`及`theme.scss`，布局使用Bootstrap flex，避免使用row&col，避免内联样式。
- 状态优先使用 signals、async pipe 或框架推荐响应式写法；模板中避免调用复杂函数。
- 表单使用 Typed Reactive Forms；在 FormGroup 内优先用 `[formControl]` + getter，不优先使用 `formControlName`。
- Import `I18N_KEYS` from `src/app/modules/share/i18n-keys` and expose it on each translated component. In templates use `i18nKeys.common.save | translate`; for `TranslateService` use `translate.instant(this.i18nKeys.common.save)`. Do not use literal or constructed translation keys.
- Keep generated request contracts untouched and regenerate them when the backend changes.
- Avoid inline styles and prefer shared styles / Material tokens.
- Put reusable frontend infrastructure in `src/app/modules/share` and import it through `src/app/modules/share/...`; do not create `src/app/share`.

## 前端模块拆分

- 以业务能力划分顶层目录 `src/app/modules/<module>/`，例如 `audit`、`notification` 和 `resource`。模块内按工作流或用例组织页面，列表、详情、编辑器和仅该用例使用的组件就近存放。可独立导航、查询或保存的工作流应有各自的页面或功能组件，分别管理加载、表单与错误状态。
- 新模块用 `<module>.routes.ts` 导出 `Routes`，由 `app.routes.ts` 使用 `loadChildren` 懒加载。模块内优先使用 standalone 组件，页面需要独立加载时使用 `loadComponent`。审核的 `flow/`、`record/` 和 `task/` 可作参考。孤立的模板片段留在当前组件，独立操作不集中在一个页面的选项卡回调或共用表单里。
- App Shell 负责登录、布局和模块入口。业务路由声明特定权限守卫，侧栏菜单同步路径、访问码和对应角色，最终权限由后端校验。
- 业务模块通过共享能力和生成客户端访问依赖，避免直接引用其他模块的页面、表单或私有状态。通用守卫、选择器和组件放 `modules/share`，生成的客户端和 DTO 沿用 `app/services` 下的生成入口。业务专用的状态和封装留在所属模块。
- 需要打包的模块，应检查路由、菜单、中英文 i18n、生成客户端及共享依赖能在目标 MiniApi 安装。不要把宿主私有实现或构建产物当作模块依赖。

## Angular 升级与 Agent Skills

- 升级前查看 [Angular 版本兼容表](https://angular.dev/reference/versions) 和 [升级指南](https://angular.dev/update-guide)。同步 Framework、CLI、Build、Material/CDK、TypeScript 和 lockfile，运行 CLI migrations，并核对 Markdown、翻译、测试等第三方包的 peer dependencies。验证冻结锁文件安装、生产构建和受影响的浏览器流程。
- [Angular Agent Skills](https://angular.dev/ai/agent-skills) 的 `angular-developer` 可按需补充 Angular API、Signals、路由、表单和可访问性用法。新建独立 Angular 应用时再参考 `angular-new-app`。Perigon 的模块目录、Material、i18n、生成客户端和打包规范以本项目为准。现有表单沿用 Typed Reactive Forms，选择新 API 时按具体功能评估。

## 页面组件和UX指南

根据数据结构选择合适的Material组件进行展示和交互，如：

- 在筛选组件中对 类别、枚举、目录等数据不多的，使用`mat-select`，以进行选择。
- 在筛选组件中对 用户或其他列表选择时，使用`Autocomplete`，以支持搜索和选择。
- 在列表的筛选页面，筛选控件通常与添加按钮放到同一行，并垂直对齐(align-items-center)，此时`<mat-form-field>`要添加`subscriptSizing="dynamic"`。
- 不要使用浏览器的弹窗提示，而是使用material dialog实现，弹窗要有适合的宽度和长度，通常长度不超过`96vh`，宽度不超过`900px`，最小宽度一般在`400px`.

在UX设计中，要遵循：

- 视觉体验交互良好。如使用不同颜色和风格标记组件或按钮。如删除要添加`error`class。
- 遵循整体的主题设计，不要自己添加自定义字体颜色。

## Recommended workflow

1. Generate or refresh request clients from backend OpenAPI through Perigon.
2. Create the page or component structure.
3. Configure route/menu(`menus.json`) and shared UI pieces.
4. Implement the TS / HTML / SCSS logic and validate the interaction states.
5. Run `pnpm build` (and optionally `pnpm start`) to verify the result.

## UX and validation checklist

- Check layout density, empty / loading / error states, and responsive behavior.
- Prefer Material components and existing theme styles over custom visual overrides.
- Keep user-visible strings in i18n and avoid hard-coded text.

## Verification

- Run `pnpm build` after frontend changes.
- Prefer `pnpm start` for live validation when the page is non-trivial.
