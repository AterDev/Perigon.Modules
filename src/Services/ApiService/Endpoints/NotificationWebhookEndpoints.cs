using System.Text.Json;
using ApiService;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.HttpResults;
using NotificationMod.Models.WebhookDtos;
using NotificationMod.Services;
using Share.Implement;

namespace ApiService.Endpoints;

/// <summary>匿名业务通知 Webhook。</summary>
public sealed class NotificationWebhookEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/webhooks/{tenantId:guid}/{sourceId:guid}", ReceiveAsync)
            .WithTags("NotificationWebhook")
            .AllowAnonymous()
            .WithMetadata(new RequestSizeLimitMetadata(1024 * 1024));
    }

    public static async Task<Results<Ok<WebhookResultDto>, JsonHttpResult<WebhookResultDto>, StatusCodeHttpResult>> ReceiveAsync(
        Guid tenantId,
        Guid sourceId,
        JsonElement payload,
        HttpContext httpContext,
        NotificationWebhookDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                httpContext.Request.ContentType?.Split(';')[0].Trim(),
                "application/json",
                StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        string? eventName = httpContext.Request.Headers["X-Gitlab-Event"].FirstOrDefault()
            ?? httpContext.Request.Headers["X-Event-Type"].FirstOrDefault();
        string? eventKey = httpContext.Request.Headers["webhook-id"].FirstOrDefault()
            ?? httpContext.Request.Headers["X-Gitlab-Event-UUID"].FirstOrDefault()
            ?? httpContext.Request.Headers["X-Event-Key"].FirstOrDefault();

        WebhookResultDto result = await dispatcher.HandleAsync(
            tenantId,
            sourceId,
            payload,
            eventName,
            eventKey,
            httpContext.Request.Headers,
            httpContext.Request.Query,
            cancellationToken);
        return result.FailedCount > 0
            ? TypedResults.Json(result, ApiJsonSerializerContext.Default.WebhookResultDto, statusCode: StatusCodes.Status502BadGateway)
            : TypedResults.Ok(result);
    }

    private sealed class RequestSizeLimitMetadata(long maxRequestBodySize) : IRequestSizeLimitMetadata
    {
        public long? MaxRequestBodySize { get; } = maxRequestBodySize;
    }
}
