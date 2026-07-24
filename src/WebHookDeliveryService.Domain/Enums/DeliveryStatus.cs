namespace WebHookDeliveryService.Domain.Enums;

public enum DeliveryStatus
{
    Pending = 0,
    Retrying = 1,
    Success = 2,
    Failed = 3,
    DeadLettered = 4
}
