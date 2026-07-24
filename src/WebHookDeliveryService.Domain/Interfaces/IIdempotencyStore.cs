namespace WebHookDeliveryService.Domain.Interfaces;

public interface IIdempotencyStore
{
    Task<bool> IsDuplicateAsync(string idempotencyKey);
    Task MarkAsync(string idempotencyKey, TimeSpan ttl);
}
