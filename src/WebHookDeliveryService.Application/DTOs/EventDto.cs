namespace WebHookDeliveryService.Application.DTOs;

public record EventIngestRequest
{
    public required string EventType { get; init; }
    public required string Payload { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record EventResponse
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = "{}";
    public string? IdempotencyKey { get; init; }
    public DateTime CreatedAt { get; init; }
}
