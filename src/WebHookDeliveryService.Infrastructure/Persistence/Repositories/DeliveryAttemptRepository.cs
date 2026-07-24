using Microsoft.EntityFrameworkCore;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Infrastructure.Persistence.Repositories;

public class DeliveryAttemptRepository(WebhookDbContext context) : IDeliveryAttemptRepository
{
    public async Task<DeliveryAttempt> CreateAsync(DeliveryAttempt attempt)
    {
        context.DeliveryAttempts.Add(attempt);
        await context.SaveChangesAsync();
        return attempt;
    }

    public async Task<List<DeliveryAttempt>> GetByDeliveryIdAsync(Guid deliveryId) =>
        await context.DeliveryAttempts
            .Where(a => a.DeliveryId == deliveryId)
            .OrderBy(a => a.AttemptNumber)
            .ToListAsync();
}
