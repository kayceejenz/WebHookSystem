namespace WebHookDeliveryService.Domain.Interfaces;

public interface IWebhookQueue
{
    Task PublishAsync(QueueMessage message);
    Task<QueueMessage?> ConsumeAsync(string consumerGroup);
    Task AckAsync(string consumerGroup, string messageId);
    Task NackAsync(string consumerGroup, string messageId);
}

public class QueueMessage
{
    public string Id { get; set; } = string.Empty;
    public string Stream { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}
