using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using SystemMod.Worker;

namespace SystemMod;

/// <summary>
/// 服务注入扩展
/// </summary>
[DisplayName("Perigon::SystemMod")]
[Description("包含用户、角色、菜单、组织、系统配置、系统日志、数据权限范围、数据权限组及用户与数据权限组关联管理")]
public static class ModuleExtensions
{
    /// <summary>
    /// 添加模块服务
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddSystemMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEntityTaskQueue<SysLogs>, EntityTaskQueue<SysLogs>>();
        builder.Services.AddSingleton<SystemLogService>();
        builder.Services.AddHostedService<SystemLogTaskHostedService>();
        builder.Services.AddHostedService<InitSystemModService>();
        return builder;
    }
}
