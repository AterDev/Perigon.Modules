using System.Net;
using System.Text.Json;
using Entity.NotificationMod;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationMod.Services;

namespace UnitTest;

public sealed class NotificationDeliveryTests
{
    [Test]
    public async Task Delivery_RendersValidJsonAndPreservesMessageContent()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler);
        var sender = new NotificationDeliverySender(
            new SingleClientFactory(client), new EphemeralDataProtectionProvider(),
            new NotificationRateLimiter(), new EventMarkdownFormatter(),
            NullLogger<NotificationDeliverySender>.Instance);
        var receiver = new NotificationReceiver
        {
            AuthType = "none", Endpoint = "https://receiver.example.test/hook", HttpMethod = "POST",
            BodyTemplate = "{\"content\":\"{content}\",\"title\":\"{title}\"}",
            TimeoutSeconds = 3, RateLimitPerMinute = 20,
        };
        var message = new NotificationEvent { EventType = "notification", Title = "Status \"ready\"", Content = "line one\nline two" };

        var result = await sender.SendAsync(receiver, message, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(handler.Count).IsEqualTo(1);
        using var json = JsonDocument.Parse(handler.Body!);
        await Assert.That(json.RootElement.GetProperty("title").GetString()).IsEqualTo("Status \"ready\"");
        await Assert.That(json.RootElement.GetProperty("content").GetString()!).Contains("line one\nline two");
    }

    [Test]
    public async Task Parser_UsesEventIdentityToSeparateDifferentDeliveries()
    {
        using var payload = JsonDocument.Parse("""{"title":"notice","content":"body"}""");
        var parser = new SourcePayloadParser();
        Guid sourceId = Guid.CreateVersion7();
        var first = parser.Parse(payload.RootElement, "notification", sourceId, null, "delivery-a");
        var duplicate = parser.Parse(payload.RootElement, "notification", sourceId, null, "delivery-a");
        var next = parser.Parse(payload.RootElement, "notification", sourceId, null, "delivery-b");

        await Assert.That(first.EventKey).IsEqualTo(duplicate.EventKey);
        await Assert.That(first.EventKey).IsNotEqualTo(next.EventKey);
        await Assert.That(first.EventType).IsEqualTo("notification");
    }

    private sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public int Count { get; private set; }
        public string? Body { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count++;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        }
    }
}
