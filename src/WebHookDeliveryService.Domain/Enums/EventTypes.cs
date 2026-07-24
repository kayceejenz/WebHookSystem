namespace WebHookDeliveryService.Domain.Enums;

public static class EventTypes
{
    public const string OrderCreated = "order.created";
    public const string OrderUpdated = "order.updated";
    public const string OrderDeleted = "order.deleted";
    public const string UserCreated = "user.created";
    public const string UserUpdated = "user.updated";
    public const string PaymentProcessed = "payment.processed";
    public const string PaymentFailed = "payment.failed";
    public const string Custom = "custom";
}
