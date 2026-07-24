using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WebHookDeliveryService.Application.Services;
using WebHookDeliveryService.Application.Validators;

namespace WebHookDeliveryService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebhookApplication(this IServiceCollection services)
    {
        services.AddScoped<WebhookService>();
        services.AddScoped<EventIngestionService>();
        services.AddScoped<DeliveryService>();
        services.AddScoped<RetryService>();
        services.AddScoped<DeadLetterService>();
        services.AddScoped<DashboardService>();

        services.AddValidatorsFromAssemblyContaining<CreateSubscriptionValidator>();

        return services;
    }
}
