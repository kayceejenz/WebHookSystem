using StackExchange.Redis;
using WebHookDeliveryService.Domain.Interfaces;

namespace WebHookDeliveryService.Infrastructure.Redis;

public class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IConnectionMultiplexer _redis;

    public RedisIdempotencyStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> IsDuplicateAsync(string idempotencyKey)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync($"wh:idem:{idempotencyKey}");
    }

    public async Task MarkAsync(string idempotencyKey, TimeSpan ttl)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"wh:idem:{idempotencyKey}", "1", ttl);
    }
}
