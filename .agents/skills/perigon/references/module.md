# MiniApi 外部模块边界

- 跨模块关联在实体中保存 ID，禁止导航到其他模块的实体，也禁止 Manager 直接读取或写入其他模块的实体集合。
- 需要验证 ID、读取展示字段或触发其他模块的业务动作时，由 Endpoint 调用相关 Manager，再把 ID、专用 DTO 或必要的快照传给消费模块。实体查询和写入留在所属模块。需要原子提交时，由 Endpoint 接线并保留当前 DbContext 和事务。
- 公共枚举目录由宿主显式组合，模块保留自己的枚举定义。

MiniApi 默认没有 `src/Modules` 业务层。普通功能直接组织在 `ApiService/Endpoints/Managers/Models/Services`，不要为了与 ApiStandard 对齐而创建模块项目。

只有用户明确要求安装/打包外部模块，且当前 Perigon CLI、模块源码和依赖已证明支持 MiniApi/Native AOT 时才使用模块工作流：

- 先检查包的目标框架、AOT/Trim 声明、反射/动态依赖、数据库提供程序和宿主假设。
- 不安装依赖 Controller、AdminService、多数据库、租户 factory 或 ApiStandard migration 资源的模块。
- 模块扩展需提供源码生成器可识别的公共注册入口；Endpoint group 仍遵循 `RestEndpointBase` 与静态 `MapEndpoints`。
- 安装后检查生成/复制的实体、Manager、Endpoint、前端、配置和项目引用，运行 build、测试和 Native AOT publish。
- 打包前打开产物检查 metadata 和文件边界，不包含宿主私有配置、凭据、构建输出或不可移植路径。

模块安装成功只证明文件落地，不证明业务、授权、数据或 AOT 正确。
