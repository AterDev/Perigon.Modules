# 技术设计

设计回答如何满足已确认需求。功能模块设计写入对应 `PDnnnn-Name.md`；根级 `Design.md` 只保留跨模块架构基线和 PD 设计索引。涉及数据迁移、安全边界、公开契约、Native AOT/Trim 或重大取舍时，在 PD 中完整记录设计。

## 建议结构

```markdown
# <功能模块>产品与技术设计

## 上下文与约束
## 目标与非目标
## 现状
## 方案概览
## 需求映射
| Requirement | Design decision | Verification |
## Endpoint、Manager 与服务职责
## 数据模型与迁移
## API / 事件 / UI 契约
## 权限与安全
## Native AOT、Trim 与序列化
## 性能、可靠性与可观测性
## 兼容、发布与回滚
## 测试策略
## 备选方案
| Decision | Rationale | Alternatives |
## 风险与待确认项
```

## 设计检查

- 每个强制需求都有设计响应或明确说明“不需要设计变化”；
- HTTP 边界使用 Minimal API endpoint group，业务逻辑进入 Manager/Service；
- DTO、API、状态转换和错误语义足以供实现与测试；
- endpoint handler、模型绑定和 OpenAPI 可被 Request Delegate Generator 静态分析；
- JSON 契约、第三方依赖、反射和动态代码说明 AOT/Trim 兼容策略及发布验证；
- 数据迁移说明兼容、恢复和回滚；MiniApi 没有内置迁移资源时明确部署责任；
- 鉴权、授权、敏感数据、性能、可观测性和测试策略覆盖设计风险；
- 记录关键决定的理由和被拒绝方案。

实现中发现设计不可行或 AOT 不兼容时，先更新设计及其影响的计划/任务，再继续。
