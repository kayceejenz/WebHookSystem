using System.Text.Json;
using StackExchange.Redis;
using WebHookDeliveryService.Domain.Constants;
using WebHookDeliveryService.Domain.Interfaces;

namespace WebHookDeliveryService.Infrastructure.Redis;

public class RedisWebhookQueue : IWebhookQueue
{
    private readonly IConnectionMultiplexer _redis;

    public RedisWebhookQueue(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task PublishAsync(QueueMessage message)
    {
        var db = _redis.GetDatabase();
        var entry = new Dictionary<string, string>
        {
            ["id"] = message.Id,
            ["stream"] = message.Stream,
            ["payload"] = message.Payload
        };
        await db.StreamAddAsync(message.Stream, entry.Select(e => new NameValueEntry(e.Key, e.Value)).ToArray());
    }

    public async Task<QueueMessage?> ConsumeAsync(string consumerGroup)
    {
        var db = _redis.GetDatabase();
        var consumerName = $"{consumerGroup}-{Environment.MachineName}";
        var results = await db.StreamReadGroupAsync(
            WebhookConstants.DeliverStream, consumerGroup, consumerName,
            count: 1, noAck: false);

        if (results.Length == 0) return null;

        var entry = results[0];
        return new QueueMessage
        {
            Id = entry.Id!,
            Stream = WebhookConstants.DeliverStream,
            Payload = entry.Values.FirstOrDefault(v => v.Name == "payload").Value!
        };
    }

    public async Task AckAsync(string consumerGroup, string messageId)
    {
        var db = _redis.GetDatabase();
        await db.StreamAcknowledgeAsync(WebhookConstants.DeliverStream, consumerGroup, messageId);
    }

    public async Task NackAsync(string consumerGroup, string messageId)
    {
        // For Redis Streams, Nack is not built-in.
        // The message will time out and be re-delivered by the consumer group.
        await Task.CompletedTask;
    }
}
