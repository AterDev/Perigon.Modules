---
name: delivery-loop
description: 执行或审计 MiniApi 迭代开发计划，持续核对 PD 需求/设计、PT 任务、代码和验证证据。用于每次 AI coding 后同步实现结果，以及检查计划、进度、阻塞、AOT 证据和完成情况。
---

# 交付闭环

把当前迭代的 PD 视为意图源，PT 视为计划和实现记录，代码和验证结果视为完成证据。checkbox 只是进度索引，不能单独证明完成。

## 工作模式

- **Implementation mode**：用户要求编码、修复或继续计划时使用。必须更新当前 PT、当前个人月度进度文件 和受影响 PD。
- **Audit mode**：用户只要求查看进度、审查计划或判定完成时使用。保持只读，报告过时文档和建议修订，不擅自写回。

## 启动

1. 读取适用的 `AGENTS.md`、`Demand.md`、`Design.md` 和 当前个人月度进度文件。
2. 确认当前 `Iter<number>-Name` 及功能模块，读取其 `PDnnnn-Name.md` 和 `PTnnnn-Name.md`。
3. Implementation mode 中没有对应 PT 时，按当前迭代下一个可用四位编号创建简洁 PT；无法确定迭代或 PD 来源时暂停。
4. 建立 `PD/REQ/SC → PT/TASK → 代码 → 验证` 的追踪关系。
5. 检查计划是否覆盖 Minimal API endpoint、Manager/Service、数据契约以及 Native AOT/Trim 风险。强制需求无任务覆盖、任务没有完成条件/验证方式或产物矛盾时，先修订计划。

## 个人月度进度文件

- 在项目仓库内执行 `git config user.name`，使用当前仓库生效的 Git 用户名作为 `<username>`；按本地当前年月 `YYYY-MM` 定位 `docs/Developoment/<username>/ProjectTracking-<YYYY-MM>.md`。每次读取或更新时重新确定用户和月份，跨月使用新文件。
- 用户名为空或不是安全的单个目录名（如包含路径分隔符、非法文件名字符，或为 `.`、`..`）时暂停进度记录并请求用户配置有效的 Git 用户名；不要猜测、改写用户名或写入其他人的目录。
- Implementation mode 在首次更新时按需创建目录和当月文件；已有文件先读取再更新，保留已有记录。Audit mode 始终只读，文件缺失时报告，不创建文件。需要历史上下文时可只读查看该用户以前月份的文件。
- 每月文件记录当前迭代和 PD/PT 引用、任务状态与进度、实现结果、验证证据、阻塞及下一步。进度以对应 PT 为依据，个人记录不代表自动汇总的全项目进度。
- 旧的 `docs/Development/ProjectTracking.md` 可作为只读历史参考，不再写入；不要迁移或覆盖其他用户的记录。PT 仍位于 `docs/Development/Iter<number>-<Name>/`。

## 状态语义

- `planned`：已定义但未开始；对应 `- [ ]`。
- `in-progress`：当前正在处理；仍保持 `- [ ]`。
- `blocked`：缺少决定、权限、依赖或存在未解决失败；保持 `- [ ]` 并记录 blocker。
- `done`：实现和任务要求的验证都已通过；对应 `- [x]`。
- `waived`：经明确决定不执行；不得勾选完成，记录原因和影响。

## 执行 loop

重复以下步骤，直到没有 ready task 或出现暂停条件：

1. 选择依赖已完成、优先级最高且范围最小的 `planned` task。
2. 核对来源、完成条件、验证方式和 AOT 影响；确认代码现状没有让任务失效。
3. 实现一个可验证增量。若发现意图错误，先同步需求/设计/任务。
4. 运行最小充分验证。涉及 endpoint 绑定、DTO/JSON、反射、依赖或发布时，使用 `native-aot` 确定是否必须执行 Release Native AOT publish 和运行验证。
5. 在 PT 记录实现结果、代码证据、精确验证命令/结果、AOT evidence、未覆盖项和下一步。只要修改代码就必须记录，包括部分成功或阻塞。
6. 只有完成条件满足且所需验证通过，才把任务改为 `[x]`。同步 PT 总状态、`done/total` 和 当前个人月度进度文件。
7. 行为、验收或设计变化同步 PD；全局范围或 AOT 架构变化再更新 `Demand.md`/`Design.md`，已交付行为再更新 Changelog。
8. 重新计算进度、ready task、阻塞项和下一步，再进入下一轮。

## 暂停条件

- 任务含义不清或需要扩大已批准范围；
- 实现与需求、设计、Minimal API/Native AOT 强制规则冲突；
- 构建、测试、AOT publish 或运行验证失败且原因尚未确定；
- 需要外部权限、凭据、基础设施或用户决定；
- 只能通过弱化断言、抑制未知 linker/AOT warning、跳过检查或隐藏错误继续。

暂停时记录 blocker、已有证据、影响任务和解除条件，不把任务标记完成。

## 验证和收敛

实现后检查：

| 维度 | 检查 |
|---|---|
| Completeness | 所有任务状态、需求代码证据、文档、AOT 影响和部署责任是否齐全 |
| Correctness | 验收场景、边界、失败路径、endpoint 契约和测试是否证明行为正确 |
| Coherence | 设计、Minimal API/AOT 约束、项目约定、公开契约和实现是否一致 |

问题分为 `CRITICAL`、`WARNING`、`SUGGESTION`。Audit mode 只报告；Implementation mode 在当前 PT 末尾追加 `Convergence` 分组和新的单调递增任务 ID，再回到执行 loop。

## 完成门槛

仅当以下条件全部满足才声明 `COMPLETE`：

- 所有纳入范围任务均为 done，或有不影响交付的 waived 记录；
- 强制需求和场景都有实现与验证证据；
- 必需 build、test、OpenAPI、AOT/Trim、容器或文档检查已通过；
- 没有未解释的 linker/AOT warning 或 `CRITICAL`；
- PT 实现记录、进度、当前个人月度进度文件 和受影响 PD 与代码一致。

最终只报告状态、`N/M` 进度、完成项、验证、阻塞/风险和下一步。环境不足或 AOT 证据缺失时使用 `INCOMPLETE` 或 `BLOCKED`。
