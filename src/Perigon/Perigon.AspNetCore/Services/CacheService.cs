using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Perigon.AspNetCore.Services;

/// <summary>
/// 简单封装对象的存储和获取
/// </summary>
public class CacheService(
    HybridCache cache,
    IOptions<CacheOption> options,
    IOptions<ComponentOption> component
)
{
    //public HybridCache Cache { get; init; } = cache;

    /// <summary>
    /// 缓存存储
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration">seconds</param>
    /// <returns></returns>
    public async Task SetValueAsync<T>(string key, T data)
    {
        await cache.SetAsync(key, data);
    }

    /// <summary>
    /// 保存到缓存
    /// </summary>
    /// <param name="data">值</param>
    /// <param name="key">键</param>
    /// <param name="expiration">绝对过期时间</param>
    /// <returns></returns>
    public async Task SetValueAsync<T>(
        string key,
        T data,
        int? expiration = null,
        int? localExpiration = null,
        HybridCacheEntryFlags? flags = null
    )
    {
        var cacheOption = options.Value;
        var cacheOptions = GetCacheEntryOptions(expiration, localExpiration, flags);
        await cache.SetAsync(key, data, cacheOptions);
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T?> GetValueAsync<T>(string key, CancellationToken cancellation = default)
    {
        var cachedValue = await cache.GetOrCreateAsync<T?>(
            key,
            factory => default,
            cancellationToken: cancellation
        );
        return cachedValue;
    }

    /// <summary>
    /// Get or create value from cache.
    /// <para>
    /// <b>Cache Stampede (Thundering Herd):</b> Automatically handled by HybridCache (concurrent requests wait for single factory execution).
    /// </para>
    /// <para>
    /// <b>Cache Penetration (Invalid Keys):</b>
    /// <br/>- If factory returns <c>null</c> (and T is nullable), it is cached. This protects against repeated queries for missing data.
    /// <br/>- If keys are random/malicious, this can fill the cache. Ensure keys are validated before calling this.
    /// </para>
    /// </summary>
    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T?>> factory,
        CancellationToken cancellation = default
    )
    {
        var cacheOptions = GetCacheEntryOptions();
        var cachedValue = await cache.GetOrCreateAsync<T?>(
            key,
            factory,
            cacheOptions,
            cancellationToken: cancellation
        );
        return cachedValue;
    }

    private HybridCacheEntryOptions GetCacheEntryOptions(
        int? expiration = null,
        int? localExpiration = null,
        HybridCacheEntryFlags? flags = null
    )
    {
        var cacheOption = options.Value;
        return new HybridCacheEntryOptions()
        {
            Expiration = expiration.HasValue
                ? TimeSpan.FromSeconds(expiration.Value)
                : TimeSpan.FromMinutes(cacheOption.Expiration),
            Flags = flags ?? GetGlobalCacheFlags(cacheOption),
            LocalCacheExpiration = localExpiration.HasValue
                ? TimeSpan.FromMinutes(localExpiration.Value)
                : TimeSpan.FromMinutes(cacheOption.LocalCacheExpiration),
        };
    }

    private HybridCacheEntryFlags GetGlobalCacheFlags(CacheOption cacheOption)
    {
        var components = component.Value;
        return components.Cache switch
        {
            CacheType.Memory => HybridCacheEntryFlags.DisableDistributedCache,
            CacheType.Redis => HybridCacheEntryFlags.DisableLocalCache,
            _ => HybridCacheEntryFlags.None,
        };
    }
}
