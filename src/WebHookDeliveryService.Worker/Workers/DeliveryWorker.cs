using System.Text.Json;
using WebHookDeliveryService.Application.Services;
using WebHookDeliveryService.Domain.Constants;
using WebHookDeliveryService.Domain.Interfaces;

namespace WebHookDeliveryService.Worker.Workers;

public class DeliveryWorker : BackgroundService
{
    private readonly ILogger<DeliveryWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebhookQueue _queue;

    public DeliveryWorker(ILogger<DeliveryWorker> logger, IServiceProvider serviceProvider, IWebhookQueue queue)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeliveryWorker starting, consuming stream: {stream}", WebhookConstants.DeliverStream);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _queue.ConsumeAsync(WebhookConstants.DefaultConsumerGroup);
                if (message is null)
                {
                    await Task.Delay(500, stoppingToken);
                    continue;
                }

                var payload = JsonSerializer.Deserialize<DeliveryPayload>(message.Payload);
                if (payload is null)
                {
                    _logger.LogWarning("Invalid delivery payload: {payload}", message.Payload);
                    await _queue.AckAsync(WebhookConstants.DefaultConsumerGroup, message.Id);
                    continue;
                }

                using var scope = _serviceProvider.CreateScope();
                var deliveryService = scope.ServiceProvider.GetRequiredService<DeliveryService>();

                var success = await deliveryService.DispatchAsync(payload.DeliveryId);

                await _queue.AckAsync(WebhookConstants.DefaultConsumerGroup, message.Id);

                _logger.LogInformation("Delivery {DeliveryId} completed: {Success}", payload.DeliveryId, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeliveryWorker");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private record DeliveryPayload(Guid DeliveryId, Guid SubscriptionId, Guid EventId);
}
