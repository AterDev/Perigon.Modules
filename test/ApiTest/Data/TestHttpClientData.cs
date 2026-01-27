using Share.Models.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TUnit.Core.Interfaces;

namespace ApiTest.Data;

public class TestHttpClientData : IAsyncInitializer, IAsyncDisposable
{
    public HttpClient HttpClient { get; private set; } = new();

    public async Task InitializeAsync()
    {
        HttpClient = (GlobalHooks.App ?? throw new NullReferenceException())
            .CreateHttpClient("AdminService");

        if (GlobalHooks.NotificationService != null)
        {
            await GlobalHooks.NotificationService
                .WaitForResourceAsync("AdminService", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));
        }

        // Authenticate once and set bearer token for subsequent requests

        var loginDto = new
        {
            Email = "admin@default.com",
            Password = "Perigon.2026",
        };

        using var resp = await HttpClient.PostAsJsonAsync("/api/systemUser/authorize", loginDto);
        resp.EnsureSuccessStatusCode();
        var token = await resp.Content.ReadFromJsonAsync<AccessTokenDto>();
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("Failed to acquire access token for tests.");
        }

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    public async ValueTask DisposeAsync()
    {
        await Console.Out.WriteLineAsync("Cleaning up HttpClient resources after tests.");
        HttpClient.Dispose();
    }
}
