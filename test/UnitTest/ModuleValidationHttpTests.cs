using AuditMod;
using AuditMod.Models.AuditRecordDtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NotificationMod;
using NotificationMod.Models.InAppNotificationDtos;
using System.Net;
using System.Net.Http.Json;

namespace UnitTest;

public sealed class ModuleValidationHttpTests
{
    [Test]
    public async Task NotificationAndAuditRequestValidation_ShouldRejectInvalidBodiesBeforeHandlers()
    {
        var calls = new CallCounters();
        await using WebApplication app = BuildApplication(calls);
        await app.StartAsync();

        try
        {
            string address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient client = new() { BaseAddress = new Uri(address) };

            HttpResponseMessage[] invalidResponses =
            [
                await client.PostAsJsonAsync("/unit/notification-validation", new InAppNotificationAddDto
                {
                    Type = "message",
                    Title = string.Empty,
                    Content = string.Empty,
                }),
                await client.PostAsJsonAsync("/unit/notification-validation", new InAppNotificationAddDto
                {
                    Type = "message",
                    Title = new string('t', 201),
                    Content = "valid",
                }),
                await client.PostAsJsonAsync("/unit/audit-validation", new AuditRecordSubmitDto
                {
                    SceneCode = string.Empty,
                    BizId = string.Empty,
                    ApplyReason = string.Empty,
                }),
                await client.PostAsJsonAsync("/unit/audit-validation", new AuditRecordSubmitDto
                {
                    SceneCode = new string('s', 101),
                    BizId = "biz-1",
                    ApplyReason = "valid",
                }),
            ];

            foreach (HttpResponseMessage response in invalidResponses)
            {
                using (response)
                {
                    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
                }
            }

            await Assert.That(calls.Notification).IsEqualTo(0);
            await Assert.That(calls.Audit).IsEqualTo(0);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static WebApplication BuildApplication(CallCounters calls)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(ModuleValidationHttpTests).Assembly.GetName().Name,
            EnvironmentName = "UnitTest",
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.AddNotificationMod();
        builder.AddAuditMod();

        WebApplication app = builder.Build();
        app.MapPost(
            "/unit/notification-validation",
            (InAppNotificationAddDto _) =>
            {
                calls.Notification++;
                return Results.Ok();
            });
        app.MapPost(
            "/unit/audit-validation",
            (AuditRecordSubmitDto _) =>
            {
                calls.Audit++;
                return Results.Ok();
            });
        return app;
    }

    private sealed class CallCounters
    {
        public int Notification { get; set; }
        public int Audit { get; set; }
    }
}
