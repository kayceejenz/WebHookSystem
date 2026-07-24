using WebHookDeliveryService.Application.DTOs;
using WebHookDeliveryService.Domain.Constants;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Application.Services;

public class WebhookService(
    IWebhookSubscriptionRepository subscriptionRepository,
    IWebhookSigner signer)
{
    public async Task<SubscriptionResponse> CreateAsync(CreateSubscriptionRequest request)
    {
        var subscription = new WebhookSubscription
        {
            Url = request.Url,
            Events = request.Events,
            Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            MaxRetries = request.MaxRetries,
            BaseDelay = TimeSpan.FromSeconds(request.BaseDelaySeconds),
            MaxDelay = TimeSpan.FromSeconds(request.MaxDelaySeconds)
        };

        await subscriptionRepository.CreateAsync(subscription);
        return MapToResponse(subscription);
    }

    public async Task<SubscriptionResponse?> GetByIdAsync(Guid id)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(id);
        return subscription is null ? null : MapToResponse(subscription);
    }

    public async Task<List<SubscriptionResponse>> GetAllAsync()
    {
        var subscriptions = await subscriptionRepository.GetAllAsync();
        return subscriptions.Select(MapToResponse).ToList();
    }

    public async Task<SubscriptionResponse?> UpdateAsync(Guid id, UpdateSubscriptionRequest request)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(id);
        if (subscription is null) return null;

        if (request.Url is not null) subscription.Url = request.Url;
        if (request.Events is not null) subscription.Events = request.Events;
        if (request.Active.HasValue) subscription.Active = request.Active.Value;
        if (request.MaxRetries.HasValue) subscription.MaxRetries = request.MaxRetries.Value;

        await subscriptionRepository.UpdateAsync(subscription);
        return MapToResponse(subscription);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(id);
        if (subscription is null) return false;

        await subscriptionRepository.DeleteAsync(id);
        return true;
    }

    public async Task<SubscriptionResponse?> RegenerateSecretAsync(Guid id)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(id);
        if (subscription is null) return null;

        subscription.Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        await subscriptionRepository.UpdateAsync(subscription);
        return MapToResponse(subscription);
    }

    private static SubscriptionResponse MapToResponse(WebhookSubscription s) => new()
    {
        Id = s.Id,
        Url = s.Url,
        Events = s.Events,
        Secret = s.Secret,
        Active = s.Active,
        MaxRetries = s.MaxRetries,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };

    private static class RandomNumberGenerator
    {
        public static byte[] GetBytes(int count)
        {
            var bytes = new byte[count];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            return bytes;
        }
    }
}
