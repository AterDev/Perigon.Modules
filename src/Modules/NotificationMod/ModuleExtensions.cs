using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Validation;
using NotificationMod.Serialization;
using NotificationMod.Services;

namespace NotificationMod;

[DisplayName("Perigon::NotificationMod")]
[Description("通知来源、规则投递与站内消息")]
public static class ModuleExtensions
{
    public static IHostApplicationBuilder AddNotificationMod(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidation();
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Add(NotificationJsonSerializerContext.Default));
        builder.Services.AddHttpClient("NotificationMod").RemoveAllResilienceHandlers();
        builder.Services.AddSingleton<NotificationRateLimiter>();
        builder.Services.AddSingleton<SourcePayloadParser>();
        builder.Services.AddSingleton<EventMarkdownFormatter>();
        builder.Services.AddScoped<NotificationDeliverySender>();
        builder.Services.AddScoped<NotificationWebhookDispatcher>();
        builder.Services.AddScoped<NotificationRecoveryService>();
        return builder;
    }
}
