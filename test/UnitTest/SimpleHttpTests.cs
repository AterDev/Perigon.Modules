using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Perigon.AspNetCore.Utils;

namespace UnitTest;

public sealed partial class SimpleHttpTests
{
    [Test]
    public async Task PostJsonAsync_WithTypeInfo_SendsAndReadsTypedJson()
    {
        var handler = new JsonResponseHandler(HttpStatusCode.OK, "{\"Value\":\"ok\"}");
        using var client = new HttpClient(handler);
        var http = new SimpleHttp { client = client };

        SimpleHttpResponse? response = await http.PostJsonAsync<SimpleHttpRequest, SimpleHttpResponse>(
            "https://example.test/notify",
            new SimpleHttpRequest { Value = "中文" },
            SimpleHttpJsonContext.Default.SimpleHttpRequest,
            SimpleHttpJsonContext.Default.SimpleHttpResponse);

        await Assert.That(handler.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(handler.RequestUri).IsEqualTo(new Uri("https://example.test/notify"));
        using JsonDocument request = JsonDocument.Parse(handler.RequestJson!);
        await Assert.That(request.RootElement.GetProperty("Value").GetString()).IsEqualTo("中文");
        await Assert.That(response?.Value).IsEqualTo("ok");
        await Assert.That(http.StatusCode).IsEqualTo(0);
    }

    [Test]
    public async Task PostJsonAsync_WithTypeInfo_RecordsFailureResponse()
    {
        var handler = new JsonResponseHandler(HttpStatusCode.BadRequest, "invalid request");
        using var client = new HttpClient(handler);
        var http = new SimpleHttp { client = client };

        SimpleHttpResponse? response = await http.PostJsonAsync<SimpleHttpRequest, SimpleHttpResponse>(
            "https://example.test/notify",
            new SimpleHttpRequest { Value = "bad" },
            SimpleHttpJsonContext.Default.SimpleHttpRequest,
            SimpleHttpJsonContext.Default.SimpleHttpResponse);

        await Assert.That(response).IsNull();
        await Assert.That(http.StatusCode).IsEqualTo((int)HttpStatusCode.BadRequest);
        await Assert.That(http.ResponseContent).IsEqualTo("invalid request");
    }

    private sealed class JsonResponseHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? RequestJson { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            RequestJson = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content),
                RequestMessage = request,
            };
        }
    }

    [JsonSerializable(typeof(SimpleHttpRequest))]
    [JsonSerializable(typeof(SimpleHttpResponse))]
    private partial class SimpleHttpJsonContext : JsonSerializerContext
    {
    }

    private sealed partial class SimpleHttpRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed partial class SimpleHttpResponse
    {
        public string Value { get; set; } = string.Empty;
    }
}
