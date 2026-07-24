namespace WebHookDeliveryService.Worker.Workers;

public class DeliveryWorker : BackgroundService
{
    private readonly ILogger<DeliveryWorker> _logger;

    public DeliveryWorker(ILogger<DeliveryWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("DeliveryWorker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
