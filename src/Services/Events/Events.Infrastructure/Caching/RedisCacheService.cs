using System.Text.Json;
using Events.Application.Ports;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Events.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer connection,
        ILogger<RedisCacheService> logger)
    {
        _database = connection.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            RedisValue value = await _database.StringGetAsync(key);

            if (value.IsNull)
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex) when (ex is RedisException or JsonException or TimeoutException)
        {
            _logger.LogWarning(
                ex,
                "Ошибка чтения ключа {CacheKey} из Redis",
                key);

            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl)
    {
        try
        {
            string json = JsonSerializer.Serialize(value);

            await _database.StringSetAsync(key, json, ttl);
        }
        catch (Exception ex) when (ex is RedisException or JsonException or TimeoutException)
        {
            _logger.LogWarning(
                ex,
                "Ошибка записи ключа {CacheKey} в Redis",
                key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning(
                ex,
                "Ошибка удаления ключа {CacheKey} из Redis",
                key);
        }
    }
}