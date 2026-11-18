using Entity.NotificationMod;
using Microsoft.AspNetCore.Http;
using NotificationMod.Services;
using Share.Exceptions;
using System.Text;
using System.Text.Json;

namespace UnitTest;

public sealed class NotificationBusinessTests
{
    [Test]
    public async Task GitLabParser_ShouldRejectUnsupportedEventTypes()
    {
        using JsonDocument payload = JsonDocument.Parse("""
            {
              "object_kind": "issue",
              "project": { "id": 42, "name": "demo" }
            }
            """);

        BusinessException? exception = null;
        try
        {
            _ = new SourcePayloadParser().Parse(
                payload.RootElement,
                "gitlab",
                Guid.CreateVersion7(),
                "issue",
                "delivery-1");
        }
        catch (BusinessException caught)
        {
            exception = caught;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.StatusCodes).IsEqualTo(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task MarkdownFormatter_ShouldDropUnsafeLinksAndKeepPayloadWithinLimit()
    {
        NotificationEvent value = new()
        {
            EventType = "event",
            Title = "Build",
            Content = new string('界', 5000),
            ProjectName = "demo",
            ActorName = "robot",
            Status = "success",
            Url = "javascript:alert(1)",
            OccurredAt = DateTimeOffset.UtcNow,
        };

        NotificationMessage message = new EventMarkdownFormatter().Format(value);

        await Assert.That(message.Link).IsEqualTo(string.Empty);
        await Assert.That(message.Content).DoesNotContain("javascript:");
        await Assert.That(Encoding.UTF8.GetByteCount(message.Content)).IsLessThanOrEqualTo(4096);
    }
}
