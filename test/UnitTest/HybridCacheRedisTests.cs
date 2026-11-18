using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceDefaults;
using StackExchange.Redis;
using Perigon.AspNetCore.Utils;
using TUnit.Core;

namespace UnitTest;

public sealed class HybridCacheRedisTests
{
    [Test]
    public async Task RedisRoundTrip_UsesBinaryStringArraySerializer()
    {
        RequireRedisIntegration();

        string key = $"perigon:unittest:binary-array:{Guid.CreateVersion7():N}";
        var options = new HybridCacheEntryOptions
        {
            Flags = HybridCacheEntryFlags.DisableLocalCache,
        };

        await using ServiceProvider writer = CreateProvider();
        HybridCache writerCache = writer.GetRequiredService<HybridCache>();
        string[] expected = ["", "中文", "😀"];

        try
        {
            await writerCache.SetAsync(key, expected, options);

            await using ServiceProvider reader = CreateProvider();
            HybridCache readerCache = reader.GetRequiredService<HybridCache>();
            string[] actual = await readerCache.GetOrCreateAsync(
                key,
                _ => new ValueTask<string[]>(["cache-miss"]),
                options);

            await Assert.That(actual).IsEquivalentTo(expected);
        }
        finally
        {
            await writerCache.RemoveAsync(key);
        }
    }

    [Test]
    public async Task SerializerRegistration_IsExactTypeOnly()
    {
        await using ServiceProvider provider = CreateProvider();

        await Assert.That(provider.GetService<IHybridCacheSerializer<string[]>>()).IsNotNull();
        await Assert.That(provider.GetService<IHybridCacheSerializer<int[]>>()).IsNull();
        await Assert.That(provider.GetServices<IHybridCacheSerializerFactory>().Select(x => x.GetType()).ToArray()).IsEquivalentTo([typeof(RejectUnknownCacheSerializerFactory)]);
    }

    [Test]
    public async Task RedisRoundTrip_UsesBinaryEnumDictionarySerializer()
    {
        RequireRedisIntegration();
        string key = $"perigon:unittest:enum-catalog:{Guid.CreateVersion7():N}";
        var options = new HybridCacheEntryOptions { Flags = HybridCacheEntryFlags.DisableLocalCache };
        Dictionary<string, List<EnumDictionary>> expected = new(StringComparer.Ordinal)
        {
            ["Status"] = [new EnumDictionary { Name = "通过", Description = "已完成", Value = 2 }],
        };

        await using ServiceProvider writer = CreateProvider();
        HybridCache writerCache = writer.GetRequiredService<HybridCache>();
        try
        {
            await writerCache.SetAsync(key, expected, options);
            await using ServiceProvider reader = CreateProvider();
            Dictionary<string, List<EnumDictionary>> actual = await reader.GetRequiredService<HybridCache>()
                .GetOrCreateAsync(key, _ => new ValueTask<Dictionary<string, List<EnumDictionary>>>(new Dictionary<string, List<EnumDictionary>>()), options);

            await Assert.That(actual["Status"][0].Description).IsEqualTo("已完成");
            await Assert.That(actual["Status"][0].Value).IsEqualTo(2);
        }
        finally
        {
            await writerCache.RemoveAsync(key);
        }
    }

    [Test]
    public async Task SerializerFactoryRejectsUnregisteredValueType()
    {
        await using var provider = CreateProvider();
        var factories = provider.GetServices<IHybridCacheSerializerFactory>().ToArray();
        await Assert.That(factories.Length).IsEqualTo(1);
        await Assert.That(factories[0]).IsTypeOf<RejectUnknownCacheSerializerFactory>();
        var factory = (RejectUnknownCacheSerializerFactory)factories[0];
        await Assert.That(() => factory.TryCreateSerializer<int[]>(out _))
            .Throws<NotSupportedException>();
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddStackExchangeRedisCache(options =>
        {
            ConfigurationOptions configuration = ConfigurationOptions.Parse("127.0.0.1:6379");
            configuration.Password = Environment.GetEnvironmentVariable("PERIGON_REDIS_PASSWORD");
            configuration.AbortOnConnectFail = false;
            options.ConfigurationOptions = configuration;
        });
        services.AddSingleton<IHybridCacheSerializerFactory, RejectUnknownCacheSerializerFactory>();
        services.AddHybridCache()
            .AddSerializer(new EnumDictionaryCacheSerializer())
            .AddSerializer(new BinaryStringArrayCacheSerializer());
        return services.BuildServiceProvider();
    }

    private static void RequireRedisIntegration()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("PERIGON_REDIS_TESTS"), "1", StringComparison.Ordinal))
        {
            Skip.Test("Set PERIGON_REDIS_TESTS=1 and PERIGON_REDIS_PASSWORD to run the Redis integration test.");
        }
    }
}




