namespace WebHookDeliveryService.Application.DTOs;

public record DeadLetterResponse
{
    public Guid Id { get; init; }
    public Guid DeliveryId { get; init; }
    public string SubscriptionUrl { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string PayloadSnapshot { get; init; } = "{}";
    public int AttemptCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public record DeadLetterListResponse
{
    public List<DeadLetterResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
}
