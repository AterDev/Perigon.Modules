using ApiService.Endpoints;
using AuditMod.Managers;
using AuditMod.Models.AuditRecordDtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationMod;
using NotificationMod.Managers;
using NotificationMod.Services;
using Perigon.AspNetCore.Constants;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace UnitTest;

public sealed class EndpointHttpTests
{
    [Test]
    public async Task ProtectedNotificationAndAuditGetEndpoints_ShouldReturnUnauthorizedBeforeBinding()
    {
        await using WebApplication app = BuildApplication();
        await app.StartAsync();

        try
        {
            string address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient client = new() { BaseAddress = new Uri(address) };

            string[] protectedGets =
            [
                "/api/admin/audit-flows?pageIndex=1&pageSize=10&orderBy[Name]=asc",
                "/api/Notification/mine/unread-count",
                "/api/audit-records/mine?pageIndex=1&pageSize=10&orderBy[CreatedTime]=desc",
                "/api/audit-tasks/mine?pageIndex=1&pageSize=10&orderBy[CreatedTime]=desc",
            ];

            foreach (string path in protectedGets)
            {
                using HttpResponseMessage response = await client.GetAsync(path);
                await Assert.That(response.StatusCode).IsEqualTo(System.Net.HttpStatusCode.Unauthorized);
            }
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Test]
    public async Task AuditRecordFilterAsParameters_ShouldBindPagingAndBusinessFields()
    {
        await using WebApplication app = BuildApplication(includeFilterProbe: true);
        await app.StartAsync();

        try
        {
            string address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient client = new() { BaseAddress = new Uri(address) };
            client.DefaultRequestHeaders.Add("X-Unit-Authenticated", "true");

            using HttpResponseMessage response = await client.GetAsync(
                "/unit/audit-filter?sceneCode=invoice.approval&bizId=biz-42&status=Approved");

            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            AuditFilterEcho? result = await response.Content.ReadFromJsonAsync<AuditFilterEcho>();
            await Assert.That(result).IsNotNull();
            await Assert.That(result!.PageIndex).IsEqualTo(1);
            await Assert.That(result.PageSize).IsEqualTo(20);
            await Assert.That(result.SceneCode).IsEqualTo("invoice.approval");
            await Assert.That(result.BizId).IsEqualTo("biz-42");
            await Assert.That(result.Status).IsEqualTo("Approved");
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static WebApplication BuildApplication(bool includeFilterProbe = false)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(EndpointHttpTests).Assembly.GetName().Name,
            EnvironmentName = "UnitTest",
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services
            .AddAuthentication("UnitTest")
            .AddScheme<AuthenticationSchemeOptions, UnauthenticatedHandler>("UnitTest", _ => { });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(WebConst.User, policy => policy.RequireAuthenticatedUser());
            options.AddPolicy(WebConst.AdminUser, policy => policy.RequireAuthenticatedUser());
        });
        builder.Services.AddDataProtection();

        // Register the endpoint graph without constructing a database or starting AppHost.
        builder.AddNotificationMod();
        builder.Services.AddScoped<NotificationManager>();
        builder.Services.AddScoped<AuditFlowManager>();
        builder.Services.AddScoped<AuditRecordManager>();
        builder.Services.AddScoped<AuditSubmissionManager>();
        builder.Services.AddScoped<AuditTaskManager>();

        WebApplication app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        NotificationEndpoints.MapEndpoints(app);
        NotificationWebhookEndpoints.MapEndpoints(app);
        AuditFlowEndpoints.MapEndpoints(app);
        AdminAuditRecordEndpoints.MapEndpoints(app);
        AuditRecordEndpoints.MapEndpoints(app);
        AuditTaskEndpoints.MapEndpoints(app);
        if (includeFilterProbe)
        {
            app.MapGet(
                    "/unit/audit-filter",
                    static ([AsParameters] AuditRecordFilterDto filter) =>
                        Results.Ok(new AuditFilterEcho
                        {
                            PageIndex = filter.PageIndex.GetValueOrDefault(1),
                            PageSize = filter.PageSize.GetValueOrDefault(20),
                            SceneCode = filter.SceneCode,
                            BizId = filter.BizId,
                            Status = filter.Status?.ToString(),
                        }))
                .RequireAuthorization(WebConst.User);
        }
        return app;
    }

    private sealed class UnauthenticatedHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!string.Equals(Request.Headers["X-Unit-Authenticated"], "true", StringComparison.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            ClaimsIdentity identity = new(
                [new Claim(ClaimTypes.NameIdentifier, "unit-test-user")],
                Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                Scheme.Name)));
        }
    }

    private sealed class AuditFilterEcho
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string? SceneCode { get; set; }
        public string? BizId { get; set; }
        public string? Status { get; set; }
    }
}






