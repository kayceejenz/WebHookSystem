namespace WebHookDeliveryService.Application.DTOs;

public record CreateSubscriptionRequest
{
    public required string Url { get; init; }
    public required List<string> Events { get; init; }
    public int MaxRetries { get; init; } = 5;
    public double BaseDelaySeconds { get; init; } = 5;
    public double MaxDelaySeconds { get; init; } = 3600;
}

public record UpdateSubscriptionRequest
{
    public string? Url { get; init; }
    public List<string>? Events { get; init; }
    public bool? Active { get; init; }
    public int? MaxRetries { get; init; }
}

public record SubscriptionResponse
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public List<string> Events { get; init; } = [];
    public string Secret { get; init; } = string.Empty;
    public bool Active { get; init; }
    public int MaxRetries { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
