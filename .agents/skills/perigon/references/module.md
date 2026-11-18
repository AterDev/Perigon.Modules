# MiniApi 外部模块边界

MiniApi 默认没有 `src/Modules` 业务层。普通功能直接组织在 `ApiService/Endpoints/Managers/Models/Services`，不要为了与 ApiStandard 对齐而创建模块项目。

只有用户明确要求安装/打包外部模块，且当前 Perigon CLI、模块源码和依赖已证明支持 MiniApi/Native AOT 时才使用模块工作流：

- 先检查包的目标框架、AOT/Trim 声明、反射/动态依赖、数据库提供程序和宿主假设。
- 不安装依赖 Controller、AdminService、多数据库、租户 factory 或 ApiStandard migration 资源的模块。
- 模块扩展需提供源码生成器可识别的公共注册入口；Endpoint group 仍遵循 `RestEndpointBase` 与静态 `MapEndpoints`。
- 安装后检查生成/复制的实体、Manager、Endpoint、前端、配置和项目引用，运行 build、测试和 Native AOT publish。
- 打包前打开产物检查 metadata 和文件边界，不包含宿主私有配置、凭据、构建输出或不可移植路径。

模块安装成功只证明文件落地，不证明业务、授权、数据或 AOT 正确。
