---
name: commit-message
description: 检查当前 diff 并生成符合 Perigon 约定的 Conventional Commit 信息。用于拟定提交标题和简短说明；不执行提交，也不为了凑提交记录自动修改跟踪或 Changelog。
---

# 提交信息

先检查 staged diff；没有 staged 变更时检查 working-tree diff。结合用户说明判断主要意图，不只按文件数量决定 type。

## 安全与交付前检查

- diff 含密码、token、私钥、生产连接或其他敏感信息时停止并警告。
- 对异常大的生成物、二进制或测试输出，先提示检查是否应忽略。
- 定位当前迭代与受影响 PT，检查 checkbox、进度、实现记录、验证/AOT 证据和 当前个人月度进度文件 是否与 diff 一致。AI coding 文档未同步时停止生成提交信息。
- 可观察行为或设计改变时，检查来源 PD 已同步；纯实现变化在 PT 记录“无 PD 影响”及理由。
- `Changelog.md` 只记已交付的用户可见行为。

## 格式

```text
<emoji> <type>(<scope>): <subject>

- <optional concise detail>
```

- `type`：`feat` `fix` `docs` `refactor` `test` `chore`。
- `scope`：优先 `api` `endpoints` `webapp` `entity` `ef` `apphost` `aspire` `aot` `perigon` `docs` `test` `templates` 或实际功能名。
- `subject` 使用简洁中文命令式，不以句号结尾。
- emoji：`feat` 🎉、`fix` 🐛、`docs` 📝、`refactor` ♻️、`test` ✅、`chore` 🔧。
- 相关变更选主要 type，次要内容放简短 body；不相关变更建议拆分。
