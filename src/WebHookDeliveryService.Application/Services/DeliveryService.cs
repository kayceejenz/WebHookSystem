using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WebHookDeliveryService.Application.DTOs;
using WebHookDeliveryService.Domain.Constants;
using WebHookDeliveryService.Domain.Enums;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Application.Services;

public class DeliveryService(
    IWebhookDeliveryRepository deliveryRepository,
    IDeliveryAttemptRepository attemptRepository,
    IWebhookSubscriptionRepository subscriptionRepository,
    IEventRepository eventRepository,
    IWebhookSigner signer,
    ILogger<DeliveryService> logger)
{
    public async Task<bool> DispatchAsync(Guid deliveryId)
    {
        var delivery = await deliveryRepository.GetByIdAsync(deliveryId);
        if (delivery is null) return false;

        var subscription = delivery.Subscription;
        var webhookEvent = delivery.Event;

        delivery.Status = DeliveryStatus.Retrying;
        delivery.AttemptCount++;
        await deliveryRepository.UpdateAsync(delivery);

        var stopwatch = Stopwatch.StartNew();
        var signature = signer.ComputeSignature(subscription.Secret,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(), webhookEvent.Payload);

        var attempt = new DeliveryAttempt
        {
            DeliveryId = deliveryId,
            AttemptNumber = delivery.AttemptCount,
            SignatureSent = $"t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()},v1={signature}"
        };

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url)
            {
                Content = new StringContent(webhookEvent.Payload, Encoding.UTF8, "application/json")
            };

            request.Headers.Add(WebhookConstants.SignatureHeader, attempt.SignatureSent!);
            request.Headers.Add(WebhookConstants.TimestampHeader,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            request.Headers.Add(WebhookConstants.EventIdHeader, webhookEvent.Id.ToString());
            request.Headers.Add(WebhookConstants.EventTypeHeader, webhookEvent.EventType);
            request.Headers.Add(WebhookConstants.DeliveryIdHeader, deliveryId.ToString());

            var response = await client.SendAsync(request);
            stopwatch.Stop();

            attempt.ResponseCode = (int)response.StatusCode;
            attempt.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            attempt.ResponseBody = await response.Content.ReadAsStringAsync();
            if (attempt.ResponseBody?.Length > WebhookConstants.MaxResponseLength)
                attempt.ResponseBody = attempt.ResponseBody[..WebhookConstants.MaxResponseLength];

            await attemptRepository.CreateAsync(attempt);

            if (WebhookConstants.SuccessCodes.Contains((int)response.StatusCode))
            {
                delivery.Status = DeliveryStatus.Success;
                delivery.CompletedAt = DateTime.UtcNow;
                delivery.LastResponseCode = (int)response.StatusCode;
                await deliveryRepository.UpdateAsync(delivery);
                return true;
            }

            attempt.Error = $"HTTP {(int)response.StatusCode}";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            attempt.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            attempt.Error = ex.Message;
        }

        await attemptRepository.CreateAsync(attempt);

        delivery.LastResponseCode = attempt.ResponseCode > 0 ? attempt.ResponseCode : null;
        delivery.LastError = attempt.Error;

        if (delivery.AttemptCount >= subscription.MaxRetries)
        {
            delivery.Status = DeliveryStatus.Failed;
            await deliveryRepository.UpdateAsync(delivery);
            return false;
        }

        delivery.Status = DeliveryStatus.Retrying;
        var delay = CalculateBackoff(subscription, delivery.AttemptCount);
        delivery.NextRetryAt = DateTime.UtcNow.Add(delay);
        await deliveryRepository.UpdateAsync(delivery);

        logger.LogWarning("Delivery {DeliveryId} attempt {Attempt} failed, retrying at {NextRetry}",
            deliveryId, delivery.AttemptCount, delivery.NextRetryAt);

        return false;
    }

    public async Task<DeliveryResponse?> GetByIdAsync(Guid id)
    {
        var delivery = await deliveryRepository.GetByIdAsync(id);
        return delivery is null ? null : MapToResponse(delivery);
    }

    public async Task<DeliveryListResponse> GetAllAsync(
        DeliveryStatus? status = null, Guid? subscriptionId = null, int skip = 0, int take = 50)
    {
        var deliveries = await deliveryRepository.GetAllAsync(status, subscriptionId, skip, take);
        var totalCount = await deliveryRepository.GetTotalCountAsync(status);
        return new DeliveryListResponse
        {
            Items = deliveries.Select(MapToResponse).ToList(),
            TotalCount = totalCount
        };
    }

    public static TimeSpan CalculateBackoff(WebhookSubscription subscription, int attempt)
    {
        var delayTicks = (long)(subscription.BaseDelay.Ticks * Math.Pow(2, attempt - 1));
        var maxTicks = subscription.MaxDelay.Ticks;
        return new TimeSpan(Math.Min(delayTicks, maxTicks));
    }

    private static DeliveryResponse MapToResponse(WebhookDelivery d) => new()
    {
        Id = d.Id,
        SubscriptionId = d.SubscriptionId,
        SubscriptionUrl = d.Subscription?.Url ?? string.Empty,
        EventId = d.EventId,
        EventType = d.Event?.EventType ?? string.Empty,
        Status = d.Status,
        AttemptCount = d.AttemptCount,
        LastResponseCode = d.LastResponseCode,
        LastError = d.LastError,
        CreatedAt = d.CreatedAt,
        CompletedAt = d.CompletedAt,
        Attempts = d.Attempts?.Select(a => new DeliveryAttemptResponse
        {
            Id = a.Id,
            AttemptNumber = a.AttemptNumber,
            ResponseCode = a.ResponseCode,
            ResponseBody = a.ResponseBody,
            DurationMs = a.DurationMs,
            SignatureSent = a.SignatureSent,
            Error = a.Error,
            AttemptedAt = a.AttemptedAt
        }).ToList() ?? []
    };
}
