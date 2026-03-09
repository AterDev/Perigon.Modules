using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;

namespace CMSMod;
/// <summary>
/// 模块服务扩展
/// </summary>
[DisplayName("Perigon::CMSMod")]
[Description("包含内容管理相关功能")]
public static class ModuleExtensions
{
    /// <summary>
    /// add module
    /// </summary>
    /// <param name="builder"></param>
    public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        return builder;
    }

    /// <summary>
    /// use module services
    /// </summary>
    /// <param name="app"></param>
    public static WebApplication UseCMSModServices(this WebApplication app)
    {
        return app;
    }
}

