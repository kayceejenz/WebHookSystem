namespace WebHookDeliveryService.Domain.Models;

public class WebhookSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
    public List<string> Events { get; set; } = [];
    public string Secret { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public int MaxRetries { get; set; } = 5;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromHours(1);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
