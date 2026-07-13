using Microsoft.Extensions.Hosting;
using System.ComponentModel;
namespace ResourceMod;

[DisplayName("Perigon::ResourceMod")]
[Description("通用资源管理模块")]
public static class ModuleExtensions
{
    /// <summary>
    /// module services or init task
    /// </summary>
    public static IHostApplicationBuilder AddResourceMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<Services.InitResourceModService>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseResourceModServices(this WebApplication app)
    {
       // custom middlewares and init task
       return app;
    }
}
