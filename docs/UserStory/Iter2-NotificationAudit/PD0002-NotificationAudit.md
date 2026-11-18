# 通知与审计模块

## 来源与范围

- Iteration: `Iter2-NotificationAudit`
- Product Design: `PD0002-NotificationAudit`
- Status: `in-progress`

本迭代在现有后端宿主中新增两个无前端依赖的官方模块：`NotificationMod` 和 `AuditMod`。

`NotificationMod` 抽取 `C:\\Users\\zpty\\work\\notifier` 中的通知定义、业务逻辑和接口，并补充面向业务处理通知及站内公告的站内消息模型。站内消息必须能按用户记录每条消息的已读/未读状态，并支持消息和公告等消息类型；它不用于用户之间的私信。

`AuditMod` 参考 `C:\\Users\\zpty\\work\\dataservices\\src\\Modules\\WorkflowMod` 的审核实体与实现，提供面向实际业务的审核记录和状态流转接口。

本迭代仅包含实体、业务逻辑、服务注册和 HTTP API，不包含 Angular 或其他前端实现。

## 技术基线

- 先完成仓库架构升级，再迁移两个业务模块：目标框架为 .NET 11（SDK 11.0.100），`Perigon.PostgreSQL` 为 0.3.1。
- 迁移期间移除 Mapster 依赖和 `MapTo`/隐式对象映射，DTO 与实体转换必须使用显式 C# 映射。
- 用户要求按 MiniApi 模板迁移到 Minimal API 和 Native AOT，采用单库，不再支持多租户。保留角色授权和按用户划分的业务数据访问边界。

## 当前实现与验证边界

- `NotificationMod` 已包含来源/规则/接收器、Webhook 事件与投递、站内消息及每用户收件状态；`AuditMod` 已包含审核流、步骤、记录、步骤明细和审核任务，并在状态推进时写入站内通知。
- `AuditMod` 的 Manager 直接使用 `Entity.NotificationMod` 中的 `InAppNotification` 和 `InAppNotificationRecipient` 实体（例如 `src/Modules/AuditMod/Managers/AuditSubmissionManager.cs`、`AuditTaskManager.cs`），因此审核模块对通知模型存在运行时耦合；当前耦合通过共享 `Entity` 项目承载，未形成独立的 `AuditMod -> NotificationMod` 项目引用。
- 13 张新增表（Notification 8 张、Audit 5 张）当前只有主键、租户过滤器和业务索引，没有数据库外键；`src/Definition/EntityFramework/PostgresMigrations/20261117011052004_NotificationAudit.cs` 中未生成 `FOREIGN KEY/REFERENCES`。跨表及跨租户一致性目前依赖 Manager 校验，后续必须决定是否补充租户复合外键，不能把现状误认为数据库级完整性。
- 已通过 .NET 11 全解决方案构建、迁移列表和空模型漂移检查；API 测试目前仅完成发现，未执行真实测试；本地 PostgreSQL 集成、迁移应用、Webhook 投递和恢复流程仍未取得运行时证据。

## 需求

- 新增独立的 `NotificationMod` 模块，保留来源通知能力并遵循本仓库的软删除和 DTO 目录规范。
- 提供站内消息实体与接口，区分消息、公告等业务消息类型。
- 为每个收件用户保存每条站内消息的已读/未读状态，支持查询未读数量、列表、详情和标记已读等实际业务操作。
- 新增独立的 `AuditMod` 模块，提供审核对象、审核记录或审核流转所需的实体和接口；状态变化必须经过服务端业务校验。
- 两个模块接入 ApiService 的 Endpoint Group、模块自动注册和 Manager 自动注册，提供可测试的后端接口。
- 不新增前端页面、路由、菜单或客户端生成代码。

## 约束

- 以当前代码、来源项目和实际依赖为准；来源项目中与本仓库框架不兼容的实现需要适配，而不是直接复制。
- 实体和 DTO 必须保持单类型单文件；DTO 放在对应模块 `Models/{Entity}Dtos` 目录，类型名以 `Dto` 结尾。
- 迁移和共享 `DefaultDbContext` 的改动由实现任务统一评审，不能为了文档同步而推断或修改未确认的 schema。
- 本地数据库集成环境未准备好时，不得把未运行的集成测试或迁移应用写成已验证；必须记录具体未验证项。

## 验收标准

- `NotificationMod`、`AuditMod` 项目可构建，模块自动注册和 Minimal API 路由可发现。
- 站内消息按当前用户安全查询，已读状态不会串用户。
- 审核接口能创建、查询并按合法状态规则推进审核状态，非法状态转换返回业务错误。
- 后端测试覆盖关键的用户隔离、已读/未读和审核状态流转场景。

## 未验证风险

- `NotificationRecoveryService` 及其控制器恢复接口已实现，但尚未在本地 PostgreSQL 集成环境验证租约、重复投递、事件幂等和失败重试语义。
- Webhook 接收、签名校验、事件去重和外部接收器投递尚未完成本地接口验证；恢复接口不能仅凭编译结果视为交付完成。
- 新增表没有数据库外键，软删除不会触发数据库级级联；如果决定补充约束，应先扫描孤儿行和跨租户关联，再新增迁移，避免影响现有租户库。
- Receiver endpoint 可由业务数据配置为 HTTP(S) 地址，尚未验证私网、loopback、云 metadata、DNS rebinding 和重定向限制，存在出站 SSRF 风险。
- HTTP 投递目前有超时和人工恢复路径，但 5xx、429、网络失败、幂等键和退避重试行为未验证，重复发送和丢失状态的处理策略仍待确认。
- 全解决方案构建通过和测试发现成功仅是静态证据；TASK-000 至 TASK-007 均保持未完成，必须完成真实业务、安全和数据库验证后才能更新状态。

### 数据库模型约定

通知与审核实体使用 Perigon.PostgreSQL 的默认表名和索引名，索引在实体上声明。上下文按 `EntityBase` 启用软删除后，实体唯一索引仅约束未删除记录。审核流的场景唯一性还要求流程已启用，业务申请的唯一性还要求记录处于待审批状态。
