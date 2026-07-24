namespace WebHookDeliveryService.Domain.Models;

public class DeadLetter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string PayloadSnapshot { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public WebhookDelivery Delivery { get; set; } = null!;
}
