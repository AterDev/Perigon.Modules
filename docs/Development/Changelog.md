# 变更记录

## 2026-11-27

- 修复 SystemMod 在干净 MiniApi 11.0.0 中的安装兼容问题。用户信息响应的 OpenAPI 模型名改为 `SysUserInfoDto`，JSON 字段保持原有契约。

- SystemMod 移除图形验证码接口和 Toolkit 项目依赖，保留邮箱验证码流程。
- 升级 Perigon.PostgreSQL 0.4.2，由生成器修复 `required` DTO 的物化工厂编译冲突。撤除两个模块 DTO 的临时映射注解并重新打包。
- 使用 Perigon CLI 11.0.0-beta1 重打五个模块。AuditMod、NotificationMod、SystemMod 为 1.1.0，CMSMod、ResourceMod 为 1.2.0。
- SystemMod 1.1.0 已通过打包检查。在干净 MiniApi 11.0.0 中安装时，仍需解决日志与查询接口、管理器方法签名及组织本地化键的兼容差异。

## 2026-11-26

- 移除 `GET /api/SysConfig/enum`。OpenAPI 的枚举文档继续使用构建时生成的 XML 元数据。

## 2026-11-19

- 管理后台新增审核流程、审核记录、我的待办和我的申请。管理员可搜索并选择审核人，申请人可跟踪进度和撤回申请。
- 新增通知管理和个人收件箱。管理员可维护草稿和受众、发布通知、管理 Webhook 来源、接收器和规则，以及查看事件和投递记录。收件人可查看未读数和阅读状态。
- 管理后台升级到 Angular 22.3.0 和 TypeScript 6.0.3。Node.js 版本范围见前端 `package.json`。
- 通知管理的六个标签页分别保留分页和未保存输入。页头刷新只更新当前标签页。
