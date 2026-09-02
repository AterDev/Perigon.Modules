---
name: module-pack
description: >-
  打包并发布 Perigon.Modules 中受影响的官方模块，读取 modules.json 计算次版本号，
  通过 Perigon.CLI 写入 zip 元数据并同步模块目录；仅在用户明确要求发布、提交或合并时执行对应 Git 操作。
---

# 模块打包与版本发布

仅在用户要求打包、发布或更新官方模块包时使用。普通模块开发、构建或测试不要触发本 skill。

## 前置检查

- 所有命令从 `C:\codes\Perigon.Modules` 根目录执行。
- 先确认 `perigon module pack -h` 包含 `-v, --version <VERSION>`。不支持时停止并更新 `Perigon.CLI`；不要通过修改 zip 内文件绕过 CLI。
- `scripts/PackModules.ps1` 是批量打包入口，目标服务为 `AdminService`。它会校验打包结果并在全部成功后同步 `modules.json`。
- 保留工作树中与本任务无关的改动；提交前只暂存本次发布相关文件。

## 确定受影响模块

先查看相对发布基线的已提交差异，并补充 staged、unstaged 和未跟踪文件：

```powershell
git diff --name-only main...HEAD
git diff --name-only --cached
git diff --name-only
git ls-files --others --exclude-standard
```

本地没有 `main` 时使用 `origin/main` 作为只读基线。根据实际模块目录解析名称，不要只依赖文件名猜测：

- `src/Modules/<Module>Mod/**`
- `src/Definition/Entity/<Module>Mod/**`
- `src/Services/*/Controllers/<Module>Mod/**`
- `src/ClientApp/WebApp/src/app/modules/<frontend-name>/**`，将前端目录与 `src/Modules/*Mod` 去掉 `Mod` 后的名称匹配。

前端 `src/app/modules/share/**` 或确实进入模块包、影响所有模块编译契约的共享代码变化，应纳入所有适用模块。仅修改文档、脚本、`modules.json` 或已有 zip 文件本身，不足以推断需要重新发布模块；如共享改动的影响无法确定，先检查依赖和包内容再决定。

如果没有模块代码变化，报告未发现需要打包的模块并停止，不要无意义地重建全部包。

## 版本规则

`modules.json` 是版本账本。必须在运行打包脚本前读取它；因为脚本完成后会用新 zip 的 metadata 覆盖汇总，不能用打包后的默认值反推旧版本。

- 当前版本必须是三段式数字版本 `major.minor.patch`。
- 本 skill 的默认发布类型是次版本：`1.0.0 → 1.1.0`，并将 patch 重置为 `0`。
- 新模块没有旧记录时使用首次发布版本 `1.0.0`。
- 只递增本次受影响模块；未受影响模块沿用旧 catalog 条目。
- CLI 未提供版本时也会使用 `1.0.0` 并发出警告；发布流程必须显式传递计算出的版本，避免把已有模块重置为 `1.0.0`。

## 打包与同步

确定模块后执行：

```powershell
pwsh ./scripts/PackModules.ps1 -Modules CMSMod,ResourceMod -Bump minor
```

把实际模块名替换为受影响集合。脚本会：

1. 在调用 CLI 前从旧 `modules.json` 计算目标版本。
2. 对每个模块执行 `perigon module pack <Module> AdminService --version <目标版本>`，存在对应前端目录时一并打包。
3. 从生成 zip 的 `metadata.json` 读取并校验 `ModuleName`、`Version`。
4. 合并未受影响模块的旧元数据，按模块名写回根目录 `modules.json`。
5. 任一模块失败时以非零状态结束，并且不写入新的 `modules.json`。

版本需要重新打包但不递增时才使用 `-Bump none`；不要手工编辑 zip 或直接把 CLI 默认的 `1.0.0` 写回已有模块。

## 验证

- 检查每个受影响 zip 内的 `metadata.json` 与 `modules.json` 中对应条目的版本一致。
- 确认未受影响模块的版本和元数据没有意外变化，并检查 zip 只包含约定的 Entity、Modules、Controllers、Frontend 和 share 内容。
- 执行 `git diff --check`，再按影响范围运行 CLI 的目标测试/构建和模块仓库必要的静态检查。
- CLI 不支持 `--version`、版本不是三段式数字、包元数据不一致或出现非预期宿主文件时停止，不提交不完整产物。

## 提交与合并

只有用户明确要求提交或合并时才执行 Git 写操作。提交前审查 staged diff，提交信息使用仓库约定的 emoji Conventional Commit，例如：

```text
🎉 feat(modules): publish versioned module packages
```

如果用户要求将 `Perigon.Modules` 的开发分支合并到 `main`，先在当前开发分支提交并确认工作树干净，再切换 `main`，优先使用 `git merge --ff-only <开发分支>`；存在分叉或冲突时停止并报告，不重置或强制覆盖。除非用户另行要求，不自动 push 远端。
