# 说明

![NuGet Version](https://img.shields.io/nuget/v/Perigon.templates?style=flat)

本仓库用于开发、集成测试和打包 Perigon CLI 与 `Perigon.templates` 官方维护的模块。仓库同时提供基于模板的 API、Angular 和 Aspire 验证宿主，确保模块可以在真实项目结构中完成端到端验证。

## 根目录

- docs: 项目文档存储目录
- scripts： 项目脚本文件目录
- src：项目代码目录
- test：测试项目目录
- .config：配置文件目录
- package_modules：已生成的模块包
- modules.json：官方模块目录元数据
- .agents：面向 AI 工具的项目开发 skill

## 代码目录src

- `src/Perigon/Perigon.AspNetCore`: 基础类库，提供基础帮助类。
- `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
- `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
- `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
- `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
  - `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
  - `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
- `src/Services/ApiService`: 是接口服务项目，基于ASP.NET Core Web API
- `src/Services/AdminService`: 后台管理服务接口项目

## 项目运行

项目基于`Aspire`，直接运行`AppHost`项目即可启动所有服务。

## 模块开发

- 模块程序集位于 `src/Modules/*Mod`，实体和控制器分别位于 `src/Definition/Entity/*Mod` 与 `src/Services/*/Controllers/*Mod`。
- 新模块开发前先阅读 `docs/UserStory` 中对应需求，并参考 `CMSMod` 或 `SystemMod` 的现有实现。
- 模块通过 Perigon CLI 打包到 `package_modules`；批量打包流程见 `scripts/PackModules.ps1`。
- AI 工具应先读取根目录 [AGENTS.md](AGENTS.md)，再按任务使用 `.agents/skills` 下的 skill。

## 文档

- [快速入门](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E5%BF%AB%E9%80%9F%E5%85%A5%E9%97%A8.html)
- [项目模板](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E9%A1%B9%E7%9B%AE%E6%A8%A1%E6%9D%BF/%E6%A6%82%E8%BF%B0.html)
- [开发规范](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E6%9C%80%E4%BD%B3%E5%AE%9E%E8%B7%B5/%E5%BC%80%E5%8F%91%E8%A7%84%E8%8C%83%E4%B8%8E%E7%BA%A6%E5%AE%9A.html)

完整文档请阅读[Perigon官方文档](https://dusi.dev/docs/Perigon.html)。
