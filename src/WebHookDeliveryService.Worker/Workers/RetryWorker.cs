using WebHookDeliveryService.Application.Services;

namespace WebHookDeliveryService.Worker.Workers;

public class RetryWorker : BackgroundService
{
    private readonly ILogger<RetryWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RetryWorker(ILogger<RetryWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RetryWorker starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var retryService = scope.ServiceProvider.GetRequiredService<RetryService>();

                var retried = await retryService.ProcessRetriesAsync();
                if (retried > 0)
                    _logger.LogInformation("Processed {Count} retries", retried);

                var deadLettered = await retryService.CheckAndDeadLetterAsync();
                if (deadLettered > 0)
                    _logger.LogInformation("Dead-lettered {Count} failed deliveries", deadLettered);

                var cleaned = await retryService.CleanupExpiredDeadLettersAsync();
                if (cleaned > 0)
                    _logger.LogInformation("Cleaned up {Count} expired dead letters", cleaned);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RetryWorker");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
