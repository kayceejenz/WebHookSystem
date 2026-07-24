using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WebHookDeliveryService.Domain.Interfaces;
using WebHookDeliveryService.Infrastructure.Persistence;
using WebHookDeliveryService.Infrastructure.Persistence.Repositories;
using WebHookDeliveryService.Infrastructure.Redis;
using WebHookDeliveryService.Infrastructure.Services;

namespace WebHookDeliveryService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebhookInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WebhookDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.AddScoped<IWebhookSubscriptionRepository, WebhookSubscriptionRepository>();
        services.AddScoped<IWebhookDeliveryRepository, WebhookDeliveryRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IDeadLetterRepository, DeadLetterRepository>();
        services.AddScoped<IDeliveryAttemptRepository, DeliveryAttemptRepository>();

        services.AddSingleton<IWebhookSigner, WebhookSigner>();
        services.AddSingleton<IWebhookQueue, RedisWebhookQueue>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();

        return services;
    }
}
