namespace WebHookDeliveryService.Application.DTOs;

public record DashboardStatsResponse
{
    public int TotalSubscriptions { get; init; }
    public int ActiveSubscriptions { get; init; }
    public int TotalDeliveries { get; init; }
    public int SuccessfulDeliveries { get; init; }
    public int FailedDeliveries { get; init; }
    public int PendingDeliveries { get; init; }
    public int RetryingDeliveries { get; init; }
    public int DeadLetteredCount { get; init; }
    public double SuccessRate { get; init; }
    public List<DeliveryTimeSeriesPoint> TimeSeries { get; init; } = [];
    public List<DeliveryResponse> RecentDeliveries { get; init; } = [];
}

public record DeliveryTimeSeriesPoint
{
    public DateTime Timestamp { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
}
