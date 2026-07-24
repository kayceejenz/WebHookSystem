using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Domain.Interfaces;

public interface IDeliveryAttemptRepository
{
    Task<DeliveryAttempt> CreateAsync(DeliveryAttempt attempt);
    Task<List<DeliveryAttempt>> GetByDeliveryIdAsync(Guid deliveryId);
}
