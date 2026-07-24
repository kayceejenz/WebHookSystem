using WebHookDeliveryService.Domain.Enums;

namespace WebHookDeliveryService.Domain.Models;

public class WebhookDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubscriptionId { get; set; }
    public Guid EventId { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public DateTime? NextRetryAt { get; set; }
    public int AttemptCount { get; set; }
    public int? LastResponseCode { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public WebhookSubscription Subscription { get; set; } = null!;
    public WebhookEvent Event { get; set; } = null!;
    public List<DeliveryAttempt> Attempts { get; set; } = [];
}
