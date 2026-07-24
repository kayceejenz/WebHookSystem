using WebHookDeliveryService.Domain.Enums;

namespace WebHookDeliveryService.Domain.Models;

public class WebhookEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
