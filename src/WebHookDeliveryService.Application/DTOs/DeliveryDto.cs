using WebHookDeliveryService.Domain.Enums;

namespace WebHookDeliveryService.Application.DTOs;

public record DeliveryResponse
{
    public Guid Id { get; init; }
    public Guid SubscriptionId { get; init; }
    public string SubscriptionUrl { get; init; } = string.Empty;
    public Guid EventId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public DeliveryStatus Status { get; init; }
    public int AttemptCount { get; init; }
    public int? LastResponseCode { get; init; }
    public string? LastError { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<DeliveryAttemptResponse> Attempts { get; init; } = [];
}

public record DeliveryAttemptResponse
{
    public Guid Id { get; init; }
    public int AttemptNumber { get; init; }
    public int ResponseCode { get; init; }
    public string? ResponseBody { get; init; }
    public int DurationMs { get; init; }
    public string? SignatureSent { get; init; }
    public string? Error { get; init; }
    public DateTime AttemptedAt { get; init; }
}

public record DeliveryListResponse
{
    public List<DeliveryResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
}
