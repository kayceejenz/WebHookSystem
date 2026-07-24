using WebHookDeliveryService.Application.DTOs;
using WebHookDeliveryService.Domain.Enums;
using WebHookDeliveryService.Domain.Interfaces;

namespace WebHookDeliveryService.Application.Services;

public class DeadLetterService(
    IDeadLetterRepository deadLetterRepository,
    IWebhookDeliveryRepository deliveryRepository,
    IEventRepository eventRepository,
    DeliveryService deliveryService)
{
    public async Task<DeadLetterListResponse> GetAllAsync(int skip = 0, int take = 50)
    {
        var items = await deadLetterRepository.GetAllAsync(skip, take);
        var total = await deadLetterRepository.GetTotalCountAsync();

        return new DeadLetterListResponse
        {
            Items = items.Select(dl => new DeadLetterResponse
            {
                Id = dl.Id,
                DeliveryId = dl.DeliveryId,
                SubscriptionUrl = dl.Delivery?.Subscription?.Url ?? string.Empty,
                EventType = dl.Delivery?.Event?.EventType ?? string.Empty,
                Reason = dl.Reason,
                PayloadSnapshot = dl.PayloadSnapshot,
                AttemptCount = dl.Delivery?.AttemptCount ?? 0,
                CreatedAt = dl.CreatedAt,
                ExpiresAt = dl.ExpiresAt
            }).ToList(),
            TotalCount = total
        };
    }

    public async Task<DeadLetterResponse?> GetByIdAsync(Guid id)
    {
        var dl = await deadLetterRepository.GetByIdAsync(id);
        if (dl is null) return null;

        return new DeadLetterResponse
        {
            Id = dl.Id,
            DeliveryId = dl.DeliveryId,
            SubscriptionUrl = dl.Delivery?.Subscription?.Url ?? string.Empty,
            EventType = dl.Delivery?.Event?.EventType ?? string.Empty,
            Reason = dl.Reason,
            PayloadSnapshot = dl.PayloadSnapshot,
            AttemptCount = dl.Delivery?.AttemptCount ?? 0,
            CreatedAt = dl.CreatedAt,
            ExpiresAt = dl.ExpiresAt
        };
    }

    public async Task<bool> ReplayAsync(Guid deadLetterId)
    {
        var deadLetter = await deadLetterRepository.GetByIdAsync(deadLetterId);
        if (deadLetter is null) return false;

        var delivery = deadLetter.Delivery;
        if (delivery is null) return false;

        delivery.Status = DeliveryStatus.Pending;
        delivery.AttemptCount = 0;
        delivery.LastResponseCode = null;
        delivery.LastError = null;
        delivery.NextRetryAt = null;
        await deliveryRepository.UpdateAsync(delivery);

        await deadLetterRepository.DeleteAsync(deadLetterId);
        return true;
    }

    public async Task<bool> DismissAsync(Guid deadLetterId)
    {
        var dl = await deadLetterRepository.GetByIdAsync(deadLetterId);
        if (dl is null) return false;

        await deadLetterRepository.DeleteAsync(deadLetterId);
        return true;
    }
}
