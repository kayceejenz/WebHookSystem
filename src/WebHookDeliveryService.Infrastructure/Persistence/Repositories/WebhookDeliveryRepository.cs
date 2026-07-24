using Microsoft.EntityFrameworkCore;
using WebHookDeliveryService.Domain.Enums;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Infrastructure.Persistence.Repositories;

public class WebhookDeliveryRepository(WebhookDbContext context) : IWebhookDeliveryRepository
{
    public async Task<WebhookDelivery?> GetByIdAsync(Guid id) =>
        await context.Deliveries
            .Include(d => d.Subscription)
            .Include(d => d.Event)
            .Include(d => d.Attempts)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<List<WebhookDelivery>> GetAllAsync(
        DeliveryStatus? status = null, Guid? subscriptionId = null, int skip = 0, int take = 50)
    {
        var query = context.Deliveries
            .Include(d => d.Subscription)
            .Include(d => d.Event)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);
        if (subscriptionId.HasValue)
            query = query.Where(d => d.SubscriptionId == subscriptionId.Value);

        return await query.OrderByDescending(d => d.CreatedAt).Skip(skip).Take(take).ToListAsync();
    }

    public async Task<List<WebhookDelivery>> GetForRetryAsync(DateTime now) =>
        await context.Deliveries
            .Include(d => d.Subscription)
            .Include(d => d.Event)
            .Where(d => d.Status == DeliveryStatus.Retrying && d.NextRetryAt <= now)
            .OrderBy(d => d.NextRetryAt)
            .Take(100)
            .ToListAsync();

    public async Task<WebhookDelivery> CreateAsync(WebhookDelivery delivery)
    {
        context.Deliveries.Add(delivery);
        await context.SaveChangesAsync();
        return delivery;
    }

    public async Task UpdateAsync(WebhookDelivery delivery)
    {
        context.Deliveries.Update(delivery);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetTotalCountAsync(DeliveryStatus? status = null)
    {
        var query = context.Deliveries.AsQueryable();
        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);
        return await query.CountAsync();
    }

    public async Task<int> GetSuccessCountAsync(DateTime from, DateTime to) =>
        await context.Deliveries
            .CountAsync(d => d.Status == DeliveryStatus.Success && d.CreatedAt >= from && d.CreatedAt <= to);

    public async Task<int> GetFailureCountAsync(DateTime from, DateTime to) =>
        await context.Deliveries
            .CountAsync(d => d.Status == DeliveryStatus.Failed && d.CreatedAt >= from && d.CreatedAt <= to);

    public async Task<Dictionary<DateTime, int>> GetDeliveriesPerHourAsync(DateTime from, DateTime to)
    {
        var deliveries = await context.Deliveries
            .Where(d => d.CreatedAt >= from && d.CreatedAt <= to)
            .Select(d => d.CreatedAt)
            .ToListAsync();

        return deliveries
            .GroupBy(d => new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0, DateTimeKind.Utc))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<List<WebhookDelivery>> GetRecentDeliveriesAsync(int count = 10) =>
        await context.Deliveries
            .Include(d => d.Subscription)
            .Include(d => d.Event)
            .OrderByDescending(d => d.CreatedAt)
            .Take(count)
            .ToListAsync();
}
