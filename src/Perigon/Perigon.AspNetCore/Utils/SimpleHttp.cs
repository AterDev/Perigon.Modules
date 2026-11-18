using System.Net.Http.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;

namespace Perigon.AspNetCore.Utils;

public class SimpleHttp
{
    public HttpClient client = new();
    public int StatusCode { get; private set; }
    public string? ResponseContent { get; private set; }

    public SimpleHttp() { }

    /// <summary>
    /// json post请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Use the overload that accepts JsonTypeInfo for Native AOT.")]
    [RequiresDynamicCode("Use the overload that accepts JsonTypeInfo for Native AOT.")]
    public async Task<T?> PostJsonAsync<T>(string url, object data)
    {
        client.Timeout = TimeSpan.FromSeconds(5);
        HttpResponseMessage res = await client.PostAsJsonAsync(url, data);
        if (res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<T>();
        }
        else
        {
            StatusCode = (int)res.StatusCode;
            ResponseContent = await res.Content.ReadAsStringAsync();
            return default;
        }
    }

    /// <summary>Posts a statically described JSON request and reads a statically described response.</summary>
    public async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo)
    {
        client.Timeout = TimeSpan.FromSeconds(5);
        using HttpResponseMessage res = await client.PostAsJsonAsync(url, data, requestTypeInfo);
        if (res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync(responseTypeInfo);
        }

        StatusCode = (int)res.StatusCode;
        ResponseContent = await res.Content.ReadAsStringAsync();
        return default;
    }
}
