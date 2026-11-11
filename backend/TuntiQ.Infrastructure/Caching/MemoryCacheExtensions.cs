using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace TuntiQ.Infrastructure.Caching;

public static class MemoryCacheExtensions
{
    /// <summary>
    /// Caches the value in serialized form and returns a freshly deserialized copy on every call.
    /// Domain objects are mutable, so a cached instance must never be shared between callers.
    /// </summary>
    public static async Task<T> GetOrCreateSerializedAsync<T>(
        this IMemoryCache cache, string key, TimeSpan duration, Func<Task<T>> factory)
    {
        var bytes = await cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = duration;
            return JsonSerializer.SerializeToUtf8Bytes(await factory());
        });

        return JsonSerializer.Deserialize<T>(bytes!)!;
    }
}
