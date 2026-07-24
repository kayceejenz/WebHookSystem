using Microsoft.EntityFrameworkCore;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Infrastructure.Persistence.Repositories;

public class DeadLetterRepository(WebhookDbContext context) : IDeadLetterRepository
{
    public async Task<DeadLetter?> GetByIdAsync(Guid id) =>
        await context.DeadLetters
            .Include(dl => dl.Delivery)
            .ThenInclude(d => d.Subscription)
            .Include(dl => dl.Delivery)
            .ThenInclude(d => d.Event)
            .FirstOrDefaultAsync(dl => dl.Id == id);

    public async Task<List<DeadLetter>> GetAllAsync(int skip = 0, int take = 50) =>
        await context.DeadLetters
            .Include(dl => dl.Delivery)
            .ThenInclude(d => d.Subscription)
            .Include(dl => dl.Delivery)
            .ThenInclude(d => d.Event)
            .OrderByDescending(dl => dl.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

    public async Task<DeadLetter> CreateAsync(DeadLetter deadLetter)
    {
        context.DeadLetters.Add(deadLetter);
        await context.SaveChangesAsync();
        return deadLetter;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await context.DeadLetters.FindAsync(id);
        if (entity is not null)
        {
            context.DeadLetters.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalCountAsync() =>
        await context.DeadLetters.CountAsync();

    public async Task<int> CleanupExpiredAsync(DateTime now)
    {
        var expired = await context.DeadLetters.Where(dl => dl.ExpiresAt <= now).ToListAsync();
        context.DeadLetters.RemoveRange(expired);
        await context.SaveChangesAsync();
        return expired.Count;
    }
}
