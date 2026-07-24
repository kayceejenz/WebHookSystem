using WebHookDeliveryService.Domain.Enums;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Domain.Interfaces;

public interface IWebhookDeliveryRepository
{
    Task<WebhookDelivery?> GetByIdAsync(Guid id);
    Task<List<WebhookDelivery>> GetAllAsync(DeliveryStatus? status = null, Guid? subscriptionId = null, int skip = 0, int take = 50);
    Task<List<WebhookDelivery>> GetForRetryAsync(DateTime now);
    Task<WebhookDelivery> CreateAsync(WebhookDelivery delivery);
    Task UpdateAsync(WebhookDelivery delivery);
    Task<int> GetTotalCountAsync(DeliveryStatus? status = null);
    Task<int> GetSuccessCountAsync(DateTime from, DateTime to);
    Task<int> GetFailureCountAsync(DateTime from, DateTime to);
    Task<Dictionary<DateTime, int>> GetDeliveriesPerHourAsync(DateTime from, DateTime to);
    Task<List<WebhookDelivery>> GetRecentDeliveriesAsync(int count = 10);
}
