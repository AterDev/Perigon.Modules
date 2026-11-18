using Entity.NotificationMod;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using NotificationMod;
using NotificationMod.Models.NotificationReceiverDtos;
using NotificationMod.Models.WebhookDtos;
using NotificationMod.Services;
using ServiceDefaults;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ApiTest;

/// <summary>
/// Fast contract tests for webhook normalization and outbound safety boundaries.
/// The API cases use the existing AppHost fixture and therefore require PostgreSQL.
/// </summary>
public sealed class NotificationWebhookSecurityTests
{
    [Test]
    public async Task Parser_ShouldUseTheProviderEventKeyForStableDeduplication()
    {
        using JsonDocument payload = JsonDocument.Parse("""
            {
              "object_kind": "pipeline",
              "project": { "id": 42, "name": "demo", "web_url": "https://git.example/demo" },
              "object_attributes": { "id": 9001, "state": "success" }
            }
            """);
        SourcePayloadParser parser = new();
        Guid sourceId = Guid.CreateVersion7();

        NormalizedEventDto first = parser.Parse(payload.RootElement, "gitlab", sourceId, "pipeline", "provider-event-1");
        NormalizedEventDto same = parser.Parse(payload.RootElement, "gitlab", sourceId, "pipeline", "provider-event-1");
        NormalizedEventDto different = parser.Parse(payload.RootElement, "gitlab", sourceId, "pipeline", "provider-event-2");

        await Assert.That(first.EventKey).IsEqualTo(same.EventKey);
        await Assert.That(first.EventKey).IsNotEqualTo(different.EventKey);
        await Assert.That(first.EventType).IsEqualTo("pipeline");
        await Assert.That(first.ProjectId).IsEqualTo(42L);
        await Assert.That(first.ObjectId).IsEqualTo("9001");
    }

    [Test]
    public async Task Parser_ShouldIncludePayloadIdentityInGitLabFallbackKey()
    {
        using JsonDocument firstPayload = JsonDocument.Parse("""
            { "object_kind": "push", "project": { "id": 42 }, "after": "aaaaaaaa", "ref": "refs/heads/main" }
            """);
        using JsonDocument secondPayload = JsonDocument.Parse("""
            { "object_kind": "push", "project": { "id": 42 }, "after": "bbbbbbbb", "ref": "refs/heads/main" }
            """);
        SourcePayloadParser parser = new();
        Guid sourceId = Guid.CreateVersion7();

        NormalizedEventDto first = parser.Parse(firstPayload.RootElement, "gitlab", sourceId, "push", null);
        NormalizedEventDto second = parser.Parse(secondPayload.RootElement, "gitlab", sourceId, "push", null);

        await Assert.That(first.EventKey).IsNotEqualTo(second.EventKey);
    }

    [Test]
    public async Task Formatter_ShouldEscapeUntrustedMarkdownAndKeepSafeLinks()
    {
        EventMarkdownFormatter formatter = new();
        NotificationEvent value = new()
        {
            EventType = "event",
            Title = "*untrusted* [title]",
            Content = "payload `code` *bold* [link]",
            ProjectName = "project",
            ActorName = "actor",
            Status = "success",
            Url = "https://git.example/demo/1",
        };

        NotificationMessage message = formatter.Format(value);

        await Assert.That(message.Content).Contains("\\*untrusted\\*");
        await Assert.That(message.Content).Contains("\\[title\\]");
        await Assert.That(message.Content).Contains("\\`code\\`");
        await Assert.That(message.Content).Contains("[查看详情](https://git.example/demo/1)");
        await Assert.That(message.Link).IsEqualTo("https://git.example/demo/1");
    }

    [Test]
    public async Task StandardNotificationClient_ShouldNotRetryUnsafePost()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.AddServiceDefaults();
        builder.AddNotificationMod();
        builder.Services.Configure<HttpStandardResilienceOptions>("NotificationMod-standard", options =>
        {
            options.Retry.MaxRetryAttempts = 1;
            options.Retry.Delay = TimeSpan.FromMilliseconds(1);
            options.Retry.ShouldRetryAfterHeader = false;
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(2);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
        });
        CountingHandler handler = new();
        builder.Services.AddHttpClient("NotificationMod")
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(3))
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        using IHost host = builder.Build();
        using HttpClient client = host.Services.GetRequiredService<IHttpClientFactory>().CreateClient("NotificationMod");
        using HttpResponseMessage response = await client.PostAsync(
            "https://receiver.example.test/hook",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.ServiceUnavailable);
        await Assert.That(handler.Calls).IsEqualTo(1);
    }

    [ClassDataSource<Data.TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task ReceiverConfiguration_ShouldRejectInvalidTemplates(Data.TestHttpClientData data)
    {
        string invalidTemplateName = $"invalid-template-{Guid.CreateVersion7():N}";
        using HttpResponseMessage invalidTemplate = await data.HttpClient.PostAsJsonAsync(
            "/api/Notification/receivers",
            new NotificationReceiverAddDto
            {
                Name = invalidTemplateName,
                Endpoint = "https://receiver.example.test/hook",
                AuthType = "none",
                BodyTemplate = "{not-json",
            });
        await Assert.That(invalidTemplate.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                RequestMessage = request,
            });
        }
    }
}
