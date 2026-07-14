using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using SystemMod.Worker;

namespace SystemMod;

/// <summary>
/// 服务注入扩展
/// </summary>
[DisplayName("Perigon::SystemMod")]
[Description("包含系统角色，用户，权限等相关功能")]
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
        builder.Services.AddSingleton<IEntityTaskQueue<SystemLogs>, EntityTaskQueue<SystemLogs>>();
        builder.Services.AddSingleton<SystemLogService>();
        builder.Services.AddHostedService<SystemLogTaskHostedService>();
        builder.Services.AddHostedService<InitSystemModService>();
        return builder;
    }
}
