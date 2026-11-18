using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Perigon.AspNetCore.Converters;
using ServiceDefaults.Middleware;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace ServiceDefaults;

/// <summary>Minimal API web pipeline retaining the tenant-aware authentication boundary.</summary>
public static class MiniApiWebExtensions
{
    public static WebApplicationBuilder AddMiniApiMiddlewareServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddAuthentication(builder.Configuration);
        services.AddThirdAuthentication(builder.Configuration);
        services.AddAuthorize();
        services.AddCors(builder.Configuration);
        services.AddRateLimiter();
        services.AddLocalizer();
        services.AddOpenApi();
        services.AddProblemDetails();
        services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost);
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        });
        return builder;
    }

    public static WebApplication UseMiniApiMiddlewareServices(this WebApplication app)
    {
        app.UseForwardedHeaders();
        if (app.Environment.IsProduction()) { app.UseHsts(); app.UseHttpsRedirection(); }
        app.UseStaticFiles();
        app.UseRequestLocalization();
        app.UseRouting();
        app.UseCors(app.Environment.IsProduction() ? WebConst.Limited : WebConst.Default);
        app.UseStatusCodePages();
        app.UseRateLimiter();
        if (!app.Environment.IsProduction()) app.MapOpenApi();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseAuthentication();
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseAuthorization();
        return app;
    }
}
