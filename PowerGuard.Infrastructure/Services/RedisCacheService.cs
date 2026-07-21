using Microsoft.Extensions.Caching.Distributed;
using PowerGuard.Application.Interfaces;
using System.Text.Json;

namespace PowerGuard.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var jsonData =  await _cache.GetStringAsync(key);

        var data = jsonData is null ? default(T) : JsonSerializer.Deserialize<T>(jsonData);

        return data;
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null,TimeSpan? slidingExpiration = null)
    {
        var options =new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(60),
            SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(30)
        };

        var jsonData = JsonSerializer.Serialize(value);

        await _cache.SetStringAsync(key, jsonData, options);
    }

   
}