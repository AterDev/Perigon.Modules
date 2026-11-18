using System.Text.Json;
using Entity.NotificationMod;

namespace NotificationMod.Services;

public sealed class NotificationDeliverySender(
    IHttpClientFactory httpClientFactory,
    IDataProtectionProvider protectionProvider,
    NotificationRateLimiter rateLimiter,
    EventMarkdownFormatter formatter,
    ILogger<NotificationDeliverySender> logger)
{
    private readonly IDataProtector _protector = protectionProvider.CreateProtector("Perigon.NotificationMod.Secrets.v1");

    public async Task<(bool Success, string? Error)> SendAsync(NotificationReceiver receiver, NotificationEvent notificationEvent, CancellationToken cancellationToken)
    {
        if (!rateLimiter.TryAcquire(receiver.Id, receiver.RateLimitPerMinute)) return (false, "receiver rate limit exceeded");
        string secret = string.Empty;
        if (!receiver.AuthType.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            try { secret = _protector.Unprotect(receiver.ProtectedSecret); }
            catch (Exception ex) { logger.LogWarning(ex, "Unable to unprotect notification receiver secret {ReceiverId}", receiver.Id); return (false, "receiver secret is invalid"); }
        }

        string endpoint = receiver.Endpoint;
        if (receiver.AuthType.Equals("query", StringComparison.OrdinalIgnoreCase))
        {
            string separator = endpoint.Contains('?') ? "&" : "?";
            endpoint += $"{separator}{Uri.EscapeDataString(string.IsNullOrWhiteSpace(receiver.AuthName) ? "token" : receiver.AuthName)}={Uri.EscapeDataString(secret)}";
        }
        using HttpRequestMessage request = new(new HttpMethod(receiver.HttpMethod), endpoint);
        NotificationMessage message = formatter.Format(notificationEvent);
        string body = receiver.BodyTemplate
            .Replace("{content}", JsonEncodedText.Encode(message.Content).ToString(), StringComparison.Ordinal)
            .Replace("{title}", JsonEncodedText.Encode(message.Title).ToString(), StringComparison.Ordinal)
            .Replace("{link}", JsonEncodedText.Encode(message.Link).ToString(), StringComparison.Ordinal)
            .Replace("{eventType}", JsonEncodedText.Encode(notificationEvent.EventType).ToString(), StringComparison.Ordinal);
        if (System.Text.Encoding.UTF8.GetByteCount(body) > 64 * 1024) return (false, "request body too large");
        request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        if (receiver.AuthType.Equals("header", StringComparison.OrdinalIgnoreCase))
        {
            string headerName = string.IsNullOrWhiteSpace(receiver.AuthName) ? "Authorization" : receiver.AuthName;
            string headerValue = string.IsNullOrWhiteSpace(receiver.AuthScheme) ? secret : $"{receiver.AuthScheme} {secret}";
            request.Headers.TryAddWithoutValidation(headerName, headerValue);
        }
        try
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(receiver.TimeoutSeconds));
            using HttpResponseMessage response = await httpClientFactory.CreateClient("NotificationMod").SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode) return (false, $"HTTP {(int)response.StatusCode}");
            await using Stream stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            using MemoryStream buffer = new();
            byte[] chunk = new byte[4096];
            int length;
            while ((length = await stream.ReadAsync(chunk, timeout.Token)) > 0)
            {
                if (buffer.Length + length > 16 * 1024) return (false, "receiver response too large");
                buffer.Write(chunk, 0, length);
            }
            string responseBody = System.Text.Encoding.UTF8.GetString(buffer.ToArray());
            if (!string.IsNullOrWhiteSpace(receiver.SuccessField) && !MatchesSuccess(responseBody, receiver.SuccessField, receiver.SuccessValue))
                return (false, "receiver success field did not match");
            return (true, null);
        }
        catch (OperationCanceledException) { return (false, cancellationToken.IsCancellationRequested ? "delivery interrupted; result unknown" : "receiver timeout"); }
        catch (HttpRequestException) { return (false, "receiver network request failed"); }
        catch (JsonException) { return (false, "receiver response format invalid"); }
        catch (IOException) { return (false, "receiver response read failed"); }
        catch (UriFormatException) { return (false, "receiver endpoint invalid"); }
        catch (InvalidOperationException) { return (false, "receiver request invalid"); }
    }

    private static bool MatchesSuccess(string body, string path, string expected)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            JsonElement current = document.RootElement;
            foreach (string segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current)) return false;
            }
            return string.Equals(current.ToString(), expected, StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException) { return false; }
    }
}
