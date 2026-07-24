using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Domain.Interfaces;

public interface IWebhookSubscriptionRepository
{
    Task<WebhookSubscription?> GetByIdAsync(Guid id);
    Task<List<WebhookSubscription>> GetAllAsync();
    Task<List<WebhookSubscription>> GetByEventTypeAsync(string eventType);
    Task<WebhookSubscription> CreateAsync(WebhookSubscription subscription);
    Task UpdateAsync(WebhookSubscription subscription);
    Task DeleteAsync(Guid id);
}
