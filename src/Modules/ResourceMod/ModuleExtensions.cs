using EntityFramework.AppDbContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
namespace ResourceMod;

[DisplayName("Perigon::ResourceMod")]
[Description("通用资源管理模块")]
public static class ModuleExtensions
{
    /// <summary>
    /// Registers module services.
    /// </summary>
    public static IHostApplicationBuilder AddResourceMod(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<Func<DefaultDbContext, CancellationToken, Task>>(InitModule.InitializeAsync);
        return builder;
    }
}
