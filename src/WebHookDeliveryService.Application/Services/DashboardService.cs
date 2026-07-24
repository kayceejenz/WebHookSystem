using WebHookDeliveryService.Application.DTOs;
using WebHookDeliveryService.Domain.Interfaces;

namespace WebHookDeliveryService.Application.Services;

public class DashboardService(
    IWebhookSubscriptionRepository subscriptionRepository,
    IWebhookDeliveryRepository deliveryRepository,
    IDeadLetterRepository deadLetterRepository)
{
    public async Task<DashboardStatsResponse> GetStatsAsync()
    {
        var allSubscriptions = await subscriptionRepository.GetAllAsync();
        var totalCount = await deliveryRepository.GetTotalCountAsync();
        var now = DateTime.UtcNow;
        var from = now.AddDays(-24);
        var successCount = await deliveryRepository.GetSuccessCountAsync(from, now);
        var failureCount = await deliveryRepository.GetFailureCountAsync(from, now);
        var pendingCount = await deliveryRepository.GetTotalCountAsync(Domain.Enums.DeliveryStatus.Pending);
        var retryingCount = await deliveryRepository.GetTotalCountAsync(Domain.Enums.DeliveryStatus.Retrying);
        var deadLettered = await deadLetterRepository.GetTotalCountAsync();
        var recentDeliveries = await deliveryRepository.GetRecentDeliveriesAsync(10);

        var timeSeries = await BuildTimeSeriesAsync(from, now);

        return new DashboardStatsResponse
        {
            TotalSubscriptions = allSubscriptions.Count,
            ActiveSubscriptions = allSubscriptions.Count(s => s.Active),
            TotalDeliveries = totalCount,
            SuccessfulDeliveries = successCount,
            FailedDeliveries = failureCount,
            PendingDeliveries = pendingCount,
            RetryingDeliveries = retryingCount,
            DeadLetteredCount = deadLettered,
            SuccessRate = totalCount > 0 ? (double)successCount / totalCount * 100 : 0,
            TimeSeries = timeSeries,
            RecentDeliveries = recentDeliveries.Select(d => new DeliveryResponse
            {
                Id = d.Id,
                SubscriptionUrl = d.Subscription?.Url ?? string.Empty,
                EventType = d.Event?.EventType ?? string.Empty,
                Status = d.Status,
                AttemptCount = d.AttemptCount,
                LastResponseCode = d.LastResponseCode,
                CreatedAt = d.CreatedAt
            }).ToList()
        };
    }

    private async Task<List<DeliveryTimeSeriesPoint>> BuildTimeSeriesAsync(DateTime from, DateTime to)
    {
        var deliveriesPerHour = await deliveryRepository.GetDeliveriesPerHourAsync(from, to);
        var successPerHour = await GetHourlyCountsAsync(from, to, Domain.Enums.DeliveryStatus.Success);
        var failurePerHour = await GetHourlyCountsAsync(from, to, Domain.Enums.DeliveryStatus.Failed);

        var result = new List<DeliveryTimeSeriesPoint>();
        var current = new DateTime(from.Year, from.Month, from.Day, from.Hour, 0, 0, DateTimeKind.Utc);

        while (current <= to)
        {
            successPerHour.TryGetValue(current, out var successes);
            failurePerHour.TryGetValue(current, out var failures);

            result.Add(new DeliveryTimeSeriesPoint
            {
                Timestamp = current,
                SuccessCount = successes,
                FailureCount = failures
            });

            current = current.AddHours(1);
        }

        return result;
    }

    private async Task<Dictionary<DateTime, int>> GetHourlyCountsAsync(
        DateTime from, DateTime to, Domain.Enums.DeliveryStatus status)
    {
        var deliveries = await deliveryRepository.GetAllAsync(status, take: 10000);
        var filtered = deliveries.Where(d => d.CreatedAt >= from && d.CreatedAt <= to);

        return filtered
            .GroupBy(d => new DateTime(d.CreatedAt.Year, d.CreatedAt.Month, d.CreatedAt.Day, d.CreatedAt.Hour, 0, 0, DateTimeKind.Utc))
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
