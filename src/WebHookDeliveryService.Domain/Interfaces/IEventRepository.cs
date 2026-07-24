using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Domain.Interfaces;

public interface IEventRepository
{
    Task<WebhookEvent?> GetByIdAsync(Guid id);
    Task<WebhookEvent?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<WebhookEvent> CreateAsync(WebhookEvent webhookEvent);
    Task<List<WebhookEvent>> GetAllAsync(int skip = 0, int take = 50);
}
