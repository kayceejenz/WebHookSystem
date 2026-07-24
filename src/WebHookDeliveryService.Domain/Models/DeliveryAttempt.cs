namespace WebHookDeliveryService.Domain.Models;

public class DeliveryAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryId { get; set; }
    public int AttemptNumber { get; set; }
    public int ResponseCode { get; set; }
    public string? ResponseBody { get; set; }
    public int DurationMs { get; set; }
    public string? SignatureSent { get; set; }
    public string? Error { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public WebhookDelivery Delivery { get; set; } = null!;
}
