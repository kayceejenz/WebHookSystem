using Microsoft.Extensions.Logging;
using WebHookDeliveryService.Application.DTOs;
using WebHookDeliveryService.Domain.Constants;
using WebHookDeliveryService.Domain.Enums;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Application.Services;

public class RetryService(
    IWebhookDeliveryRepository deliveryRepository,
    IDeadLetterRepository deadLetterRepository,
    DeliveryService deliveryService,
    ILogger<RetryService> logger)
{
    public async Task<int> ProcessRetriesAsync()
    {
        var dueDeliveries = await deliveryRepository.GetForRetryAsync(DateTime.UtcNow);
        var processed = 0;

        foreach (var delivery in dueDeliveries)
        {
            try
            {
                await deliveryService.DispatchAsync(delivery.Id);
                processed++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing retry for delivery {DeliveryId}", delivery.Id);
            }
        }

        return processed;
    }

    public async Task<int> CheckAndDeadLetterAsync()
    {
        var failedDeliveries = await deliveryRepository.GetAllAsync(DeliveryStatus.Failed, take: 100);
        var deadLettered = 0;

        foreach (var delivery in failedDeliveries)
        {
            var deadLetter = new DeadLetter
            {
                DeliveryId = delivery.Id,
                Reason = delivery.LastError ?? "All retry attempts exhausted",
                PayloadSnapshot = delivery.Event?.Payload ?? "{}",
                ExpiresAt = DateTime.UtcNow.AddDays(WebhookConstants.DeadLetterTtlDays)
            };

            await deadLetterRepository.CreateAsync(deadLetter);
            delivery.Status = DeliveryStatus.DeadLettered;
            await deliveryRepository.UpdateAsync(delivery);
            deadLettered++;
        }

        return deadLettered;
    }

    public async Task<int> CleanupExpiredDeadLettersAsync()
    {
        return await deadLetterRepository.CleanupExpiredAsync(DateTime.UtcNow);
    }
}
