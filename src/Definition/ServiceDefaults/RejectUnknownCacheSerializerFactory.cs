using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Hybrid;

namespace ServiceDefaults;

/// <summary>Prevent unsupported distributed cache values from falling back to JSON.</summary>
public sealed class RejectUnknownCacheSerializerFactory : IHybridCacheSerializerFactory
{
    public bool TryCreateSerializer<T>([NotNullWhen(true)] out IHybridCacheSerializer<T>? serializer)
    {
        serializer = null;
        if (typeof(T) == typeof(string) || typeof(T) == typeof(byte[]))
            return false; // HybridCache's built-in binary serializers.
        throw new NotSupportedException($"No binary cache serializer is registered for {typeof(T)}.");
    }
}

