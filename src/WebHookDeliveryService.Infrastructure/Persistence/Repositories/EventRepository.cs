using Microsoft.EntityFrameworkCore;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Infrastructure.Persistence.Repositories;

public class EventRepository(WebhookDbContext context) : IEventRepository
{
    public async Task<WebhookEvent?> GetByIdAsync(Guid id) =>
        await context.Events.FindAsync(id);

    public async Task<WebhookEvent?> GetByIdempotencyKeyAsync(string idempotencyKey) =>
        await context.Events.FirstOrDefaultAsync(e => e.IdempotencyKey == idempotencyKey);

    public async Task<WebhookEvent> CreateAsync(WebhookEvent webhookEvent)
    {
        context.Events.Add(webhookEvent);
        await context.SaveChangesAsync();
        return webhookEvent;
    }

    public async Task<List<WebhookEvent>> GetAllAsync(int skip = 0, int take = 50) =>
        await context.Events
            .OrderByDescending(e => e.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
}
