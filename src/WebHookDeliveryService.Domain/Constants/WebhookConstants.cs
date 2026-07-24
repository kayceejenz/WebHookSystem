namespace WebHookDeliveryService.Domain.Constants;

public static class WebhookConstants
{
    public const string SignatureHeader = "X-Webhook-Signature";
    public const string TimestampHeader = "X-Webhook-Timestamp";
    public const string EventIdHeader = "X-Webhook-Event-Id";
    public const string EventTypeHeader = "X-Webhook-Event-Type";
    public const string DeliveryIdHeader = "X-Webhook-Delivery-Id";

    public const int DefaultMaxRetries = 5;
    public const double DefaultBaseDelaySeconds = 5;
    public const double DefaultMaxDelaySeconds = 3600; // 1 hour
    public const int SignatureToleranceSeconds = 300; // 5 minutes

    public const int MaxResponseLength = 4096;
    public const int DeadLetterTtlDays = 30;

    public const string DeliverStream = "wh:deliver";
    public const string RetryStream = "wh:retry";
    public const string DeadLetterStream = "wh:deadletter";

    public const string DefaultConsumerGroup = "wh-workers";
    public const string RetryConsumerGroup = "wh-retry-workers";
    public const string DLQConsumerGroup = "wh-dlq-workers";

    public static readonly HashSet<int> SuccessCodes = [200, 201, 202, 203, 204];
    public static readonly HashSet<int> RetryableCodes = [408, 429, 500, 502, 503, 504];
}
