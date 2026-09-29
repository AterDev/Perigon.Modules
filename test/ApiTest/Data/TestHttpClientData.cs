using Share.Models.Auth;
using Perigon.AspNetCore.Constants;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SystemMod.Models;
using SystemMod.Models.SysRoleDtos;
using SystemMod.Models.SysUserDtos;
using TUnit.Core.Interfaces;

namespace ApiTest.Data;

public class TestHttpClientData : IAsyncInitializer, IAsyncDisposable
{
    private static readonly SemaphoreSlim UserRoleGate = new(1, 1);
    private readonly List<HttpClient> _systemUserClients = [];

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

        using var resp = await HttpClient.PostAsJsonAsync("/api/SysUser/authorize", loginDto);
        resp.EnsureSuccessStatusCode();
        var token = await resp.Content.ReadFromJsonAsync<AccessTokenDto>();
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("Failed to acquire access token for tests.");
        }

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    /// <summary>
    /// Creates and logs in a SysUser with the ordinary User role for API tests.
    /// </summary>
    public async Task<(HttpClient Client, Guid UserId)> CreateSysUserClientAsync()
    {
        Guid userRoleId = await EnsureUserRoleAsync();
        string suffix = Guid.CreateVersion7().ToString("N");
        string email = $"user-{suffix}@default.com";
        string password = "Perigon.User.2026";
        SysUserAddDto addDto = new()
        {
            UserName = $"user-{suffix[..20]}",
            Email = email,
            Password = password,
            RoleIds = [userRoleId]
        };

        using HttpResponseMessage createResponse = await HttpClient.PostAsJsonAsync(
            "/api/SysUser",
            addDto);
        await EnsureStatusAsync(createResponse, HttpStatusCode.Created, "/api/SysUser");
        using JsonDocument created = await ReadJsonAsync(createResponse);
        Guid userId = created.RootElement.GetProperty("id").GetGuid();

        HttpClient client = (GlobalHooks.App ?? throw new NullReferenceException())
            .CreateHttpClient("AdminService");
        using HttpResponseMessage loginResponse = await client.PostAsJsonAsync(
            "/api/SysUser/authorize",
            new SysLoginDto { Email = email, Password = password });
        await EnsureStatusAsync(loginResponse, HttpStatusCode.OK, "/api/SysUser/authorize");
        AccessTokenDto token = await loginResponse.Content.ReadFromJsonAsync<AccessTokenDto>()
            ?? throw new InvalidOperationException("SysUser login returned an empty access token.");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
        _systemUserClients.Add(client);
        return (client, userId);
    }

    private async Task<Guid> EnsureUserRoleAsync()
    {
        await UserRoleGate.WaitAsync();
        try
        {
            using HttpResponseMessage listResponse = await HttpClient.GetAsync(
                "/api/SysRole?nameValue=User&pageSize=100");
            await EnsureStatusAsync(listResponse, HttpStatusCode.OK, "/api/SysRole");
            using JsonDocument roles = await ReadJsonAsync(listResponse);
            JsonElement existing = roles.RootElement.GetProperty("data").EnumerateArray()
                .FirstOrDefault(item => item.GetProperty("nameValue").GetString() == WebConst.User);
            if (existing.ValueKind != JsonValueKind.Undefined)
            {
                return existing.GetProperty("id").GetGuid();
            }

            using HttpResponseMessage createResponse = await HttpClient.PostAsJsonAsync(
                "/api/SysRole",
                new SysRoleAddDto
                {
                    Name = WebConst.User,
                    NameValue = WebConst.User,
                    IsSystem = true
                });
            await EnsureStatusAsync(createResponse, HttpStatusCode.Created, "/api/SysRole");
            using JsonDocument created = await ReadJsonAsync(createResponse);
            return created.RootElement.GetProperty("id").GetGuid();
        }
        finally
        {
            UserRoleGate.Release();
        }
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<JsonDocument>()
            ?? throw new InvalidOperationException("The API returned an empty JSON response.");
    }

    private static async Task EnsureStatusAsync(
        HttpResponseMessage response,
        HttpStatusCode expected,
        string path)
    {
        if (response.StatusCode != expected)
        {
            string body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"{path} returned {(int)response.StatusCode}: {body}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Console.Out.WriteLineAsync("Cleaning up HttpClient resources after tests.");
        foreach (HttpClient client in _systemUserClients)
        {
            client.Dispose();
        }

        HttpClient.Dispose();
    }
}
