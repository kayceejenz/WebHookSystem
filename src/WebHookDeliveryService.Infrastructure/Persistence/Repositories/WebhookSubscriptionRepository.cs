using Microsoft.EntityFrameworkCore;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Infrastructure.Persistence.Repositories;

public class WebhookSubscriptionRepository(WebhookDbContext context) : IWebhookSubscriptionRepository
{
    public async Task<WebhookSubscription?> GetByIdAsync(Guid id) =>
        await context.Subscriptions.FindAsync(id);

    public async Task<List<WebhookSubscription>> GetAllAsync() =>
        await context.Subscriptions.OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task<List<WebhookSubscription>> GetByEventTypeAsync(string eventType) =>
        await context.Subscriptions
            .Where(x => x.Active && x.Events.Contains(eventType))
            .ToListAsync();

    public async Task<WebhookSubscription> CreateAsync(WebhookSubscription subscription)
    {
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();
        return subscription;
    }

    public async Task UpdateAsync(WebhookSubscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        context.Subscriptions.Update(subscription);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await context.Subscriptions.FindAsync(id);
        if (entity is not null)
        {
            context.Subscriptions.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
