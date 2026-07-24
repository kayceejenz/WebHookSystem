using System.Text.Json;
using WebHookDeliveryService.Application.DTOs;
using WebHookDeliveryService.Domain.Constants;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Application.Services;

public class EventIngestionService(
    IEventRepository eventRepository,
    IWebhookSubscriptionRepository subscriptionRepository,
    IWebhookDeliveryRepository deliveryRepository,
    IIdempotencyStore idempotencyStore,
    IWebhookQueue queue)
{
    public async Task<EventResponse?> IngestAsync(EventIngestRequest request)
    {
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            if (await idempotencyStore.IsDuplicateAsync(request.IdempotencyKey))
                return null;

            var existing = await eventRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);
            if (existing is not null)
            {
                await idempotencyStore.MarkAsync(request.IdempotencyKey, TimeSpan.FromHours(24));
                return MapToResponse(existing);
            }
        }

        var webhookEvent = new WebhookEvent
        {
            EventType = request.EventType,
            Payload = request.Payload,
            IdempotencyKey = request.IdempotencyKey
        };

        await eventRepository.CreateAsync(webhookEvent);

        if (!string.IsNullOrEmpty(request.IdempotencyKey))
            await idempotencyStore.MarkAsync(request.IdempotencyKey, TimeSpan.FromHours(24));

        var subscriptions = await subscriptionRepository.GetByEventTypeAsync(request.EventType);

        foreach (var subscription in subscriptions)
        {
            var delivery = new WebhookDelivery
            {
                SubscriptionId = subscription.Id,
                EventId = webhookEvent.Id,
                Status = Domain.Enums.DeliveryStatus.Pending
            };

            await deliveryRepository.CreateAsync(delivery);

            var message = new QueueMessage
            {
                Id = delivery.Id.ToString(),
                Stream = WebhookConstants.DeliverStream,
                Payload = JsonSerializer.Serialize(new
                {
                    deliveryId = delivery.Id,
                    subscriptionId = subscription.Id,
                    eventId = webhookEvent.Id
                })
            };

            await queue.PublishAsync(message);
        }

        return MapToResponse(webhookEvent);
    }

    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var evt = await eventRepository.GetByIdAsync(id);
        return evt is null ? null : MapToResponse(evt);
    }

    public async Task<List<EventResponse>> GetAllAsync(int skip = 0, int take = 50)
    {
        var events = await eventRepository.GetAllAsync(skip, take);
        return events.Select(MapToResponse).ToList();
    }

    private static EventResponse MapToResponse(WebhookEvent e) => new()
    {
        Id = e.Id,
        EventType = e.EventType,
        Payload = e.Payload,
        IdempotencyKey = e.IdempotencyKey,
        CreatedAt = e.CreatedAt
    };
}
