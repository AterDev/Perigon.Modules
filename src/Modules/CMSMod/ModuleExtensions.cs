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
        return builder;
    }
}

