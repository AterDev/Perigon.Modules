---
name: code-review
description: 对 MiniApi 代码或变更执行只读审查，检查需求符合性、Minimal API 契约、Native AOT/Trim、安全、性能、测试与完成情况。用于 PR/差异审查、质量门、计划完成审计或风险识别；不在审查任务中直接修复问题。
---

# MiniApi 代码审查

审查以本次变更及直接影响面为主，以需求、设计、`AGENTS.md`、代码和实际验证为证据。不把个人风格偏好伪装成缺陷。

## 准备

1. 确认审查基线和变更范围，保留工作树中的用户改动。
2. 读取 `perigon` 及相关 reference。涉及 endpoint、DTO、反射、依赖、EF 或发布时读 `native-aot`；涉及测试证据时读 `test`；涉及 AppHost 生命周期时使用可用的 Aspire skill。
3. 定位当前迭代 PD/PT，建立 `PD/REQ/SC → PT/TASK → diff → test/AOT evidence` 映射；AI coding 不存在跟踪时视为缺口。
4. 按风险运行最小必要构建、测试或静态检查；记录未运行原因。

## 审查顺序

1. **Completeness**：需求、场景和任务是否有实现证据；PT、当前个人月度进度文件、PD 是否同步；是否有范围漂移。
2. **Minimal API contracts**：endpoint group、路由约束、参数绑定来源、授权与 metadata 顺序、OperationId、状态码、ProblemDetails 和 OpenAPI 是否稳定；handler 是否保持静态可分析。
3. **Native AOT and Trim**：Request Delegate Generator、JSON 元数据、反射/动态代码、DI 注册、EF 模型和第三方依赖是否兼容；是否有未解释 linker/AOT warning；AOT-sensitive 变化是否有 publish 和运行证据。
4. **Correctness and security**：正常/边界/失败/并发/幂等行为、输入验证、鉴权授权、敏感信息和数据破坏风险。
5. **Architecture**：endpoint 只承担 HTTP 边界，业务逻辑进入 Manager/Service，不直接堆积 DbContext 流程；请求/响应模型不泄漏实体。
6. **Performance and operations**：无 N+1、无界查询/重试、阻塞 I/O、资源泄漏；启动、内存、日志、trace 和健康检查支持目标场景。
7. **Tests and maintainability**：测试映射场景并实际通过；真实 JSON/HTTP 路径覆盖；行为/设计变化已同步 PD，每次 coding 已同步 PT。

## 严重度与结论

- `CRITICAL`：数据/安全破坏、强制需求缺失、AOT 发布不可用或重大故障。
- `WARNING`：功能错误、关键边界/测试缺失、明显 AOT/性能/兼容风险。
- `SUGGESTION`：不影响正确性与交付的可维护性改进。

每条 finding 给出精确文件和行、触发条件、影响、证据和建议。

```text
REVIEW_STATUS: PASS | FAIL | INCOMPLETE
NEXT_ACTION: NONE | ENGINEER_FIX_REQUIRED | EVIDENCE_REQUIRED

Coverage:
- requirements: covered/total or not-available
- tasks: done/total or not-available

Verification:
- <exact command>: passed | failed | not-run
- Native AOT: passed | failed | not-affected | not-run
```

只有无 `CRITICAL`/`WARNING`、必需验证通过且证据充足时返回 `PASS`。缺少环境或 AOT 证据时返回 `INCOMPLETE`。
