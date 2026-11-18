# Project Tracking

## Current Iteration

- Iteration: [Iter2-NotificationAudit](Iter2-NotificationAudit/PT0002-NotificationAudit.md)
- Overall status: `in-progress`
- Scope: NotificationMod 站内通知与 AuditMod 审计后端模块，仅包含实体、业务逻辑和接口
- Current task: `TASK-000`~`TASK-007` 完成必要业务、安全、数据库和运行时验证；模块代码与全解决方案构建已通过，但迭代仍保持 `in-progress`
- Constraint: 不包含前端；遵循租户隔离、软删除和 DTO 单类型单文件约定

## Iterations

| Iteration | PT | Status | Progress |
|---|---|---|---|
| [Iter1-EntityRelationship](Iter1-EntityRelationship/PT0001-EntityRelationship.md) | [PT0001](Iter1-EntityRelationship/PT0001-EntityRelationship.md) | done | 7/7 |
| [Iter2-NotificationAudit](Iter2-NotificationAudit/PT0002-NotificationAudit.md) | [PT0002](Iter2-NotificationAudit/PT0002-NotificationAudit.md) | in-progress | 0/8 |

## Recent Implementation

- 仓库技术基线为 .NET 11（SDK 11.0.100）和 `Perigon.PostgreSQL 0.3.1`；相关 `.agents/skills` 已同步版本和提供程序约束。
- NotificationMod 和 AuditMod 已实现并接入 AdminService；AuditMod Manager 直接写入 `Entity.NotificationMod` 的站内消息实体，存在通知模型运行时依赖。
- `dotnet build Perigon.Modules.slnx --no-restore -v:q` 已通过，0 警告、0 错误，包含 NotificationMod、AuditMod、AdminService、ApiService、AppHost 和 ApiTest。
- 新迁移包含 Notification 8 张表和 Audit 5 张表；当前 13 张表没有数据库外键，跨表和跨租户一致性仍由业务层校验保障。
- 本地 PostgreSQL 集成尚未执行，尚未取得真实数据库集成测试、迁移应用和接口行为证据；本迭代不以生产部署、容器发布或运维验证作为交付条件。

## Verification Status

- `pwsh -NoProfile -File scripts/Migrations.ps1 List` 已通过，列出 `20261030070033728_InitialPostgreSQL` 与 `20261117011052004_NotificationAudit`。
- 临时 `CompatibilityCheck` migration model diff 为空，Up/Down 无操作且 snapshot 未改变，当前模型未发现额外空漂移。
- `dotnet test --project test/ApiTest/ApiTest.csproj --no-build --list-tests` 仅发现 43 个测试；实际数据库/接口测试未执行，不构成测试套件通过证据。
- 临时 .NET 11 控制台检查 GitLab 不同事件标识生成不同事件键及不安全链接抑制 — passed；该检查不替代真实数据库测试或完整 API 测试。
- Webhook 接收/签名/去重/投递和 `NotificationRecoveryService` 的租约、重试、恢复仍未取得本地数据库/接口运行证据；迁移应用和接口行为仍未验证。
- 测试发现、编译成功、迁移模型空漂移检查和临时控制台检查只证明静态/局部行为，不能替代 TASK-000~TASK-007 的业务验收，因此所有任务继续保持未完成。

## Current Risks

- 13 张新表没有数据库外键；若补充约束，必须先扫描孤儿行和跨租户关联，并设计租户复合外键与软删除策略。
- AuditMod 对 NotificationMod 实体存在运行时耦合；当前通过共享 Entity 项目编译，后续模块拆包或独立发布前必须明确该依赖边界。
- Receiver endpoint 允许租户配置 HTTP(S) 地址；尚未验证私网、loopback、云 metadata 地址、DNS rebinding 和重定向限制，存在出站 SSRF 风险。
- 外部投递当前主要依赖超时和手工恢复；HTTP 5xx、429、连接失败、幂等键和退避重试策略尚未通过真实接收器验证，重复投递或丢失状态风险未关闭。
- PT0002 保持 `in-progress`，TASK-000~TASK-007 全部未完成；不得以构建成功或测试发现成功替代真实数据库/接口验证。
