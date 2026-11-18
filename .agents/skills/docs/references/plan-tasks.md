# PT 计划、任务与实现记录

PT 必须写明迭代、PT 编号、来源 PD 链接、目标、范围、总状态和 `done/total` 进度。

## Plan

计划覆盖技术上下文、受影响 endpoint/manager/service、未知项、数据与契约顺序、AOT/Trim 与序列化兼容策略、依赖关系、验证检查点和发布/回滚安排。未知技术先记录 `Decision / Rationale / Alternatives`，解决后再拆任务。

## Tasks

```markdown
- [ ] TASK-001 [REQ-001] 在 `src/...` 完成可独立验证的动作
  - Depends on: none
  - Done when: 可观察结果
  - Verify: `具体命令` 或人工验收步骤
```

- 按用户场景或可交付增量分组，避免巨型任务。
- 只有修改不同文件且没有未完成依赖的任务才标记为可并行。
- 测试任务来自验收场景或风险。
- endpoint、DTO、序列化、依赖或发布相关任务必须包含 AOT 影响判断；存在风险时把 Release Native AOT publish 作为完成条件。
- 每个强制需求至少映射到一个实现任务和一个验证动作。

## AI coding 后的实现记录

```markdown
## 实现记录

### `YYYY-MM-DD` — `TASK-nnn` / 简短结果
- Status: `done | in-progress | blocked`
- Implementation: 受影响组件与可观察结果
- Code evidence: `path/to/file:line`
- Verification:
  - `<exact command>` — `passed | failed | not-run`
- AOT evidence: `not-affected`，或 publish/运行命令与结果
- Documentation: 已同步的 PD/索引/Changelog，或“无行为/设计影响”及理由
- Remaining: 未覆盖项、风险、blocker 和下一步
```

同时更新总状态、进度、task checkbox 和 当前个人月度进度文件。记录已发生事实，不粘贴长日志或声称未运行的测试已通过。

## 实现前分析

检查需求、设计、计划、任务是否矛盾，是否存在覆盖或验证缺口，Minimal API/Native AOT/Trim/安全/性能约束是否进入计划，依赖与并行标记是否真实，以及是否仍有会改变实现的待确认项。`CRITICAL` 未解决时不要开始实现。
