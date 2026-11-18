using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Validation;
using System.ComponentModel;

namespace AuditMod;

[DisplayName("Perigon::AuditMod")]
[Description("业务审核流程、审核记录与审核任务")]
public static class ModuleExtensions
{
    public static IHostApplicationBuilder AddAuditMod(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidation();
        return builder;
    }
}
