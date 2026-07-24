using WebHookDeliveryService.Application.Services;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Tests;

public class DeliveryServiceTests
{
    [Fact]
    public void CalculateBackoff_FirstAttempt_ReturnsBaseDelay()
    {
        var subscription = new WebhookSubscription
        {
            BaseDelay = TimeSpan.FromSeconds(5),
            MaxDelay = TimeSpan.FromHours(1)
        };

        var delay = DeliveryService.CalculateBackoff(subscription, 1);

        Assert.Equal(TimeSpan.FromSeconds(5), delay);
    }

    [Fact]
    public void CalculateBackoff_SecondAttempt_DoublesDelay()
    {
        var subscription = new WebhookSubscription
        {
            BaseDelay = TimeSpan.FromSeconds(5),
            MaxDelay = TimeSpan.FromHours(1)
        };

        var delay = DeliveryService.CalculateBackoff(subscription, 2);

        Assert.Equal(TimeSpan.FromSeconds(10), delay);
    }

    [Fact]
    public void CalculateBackoff_ThirdAttempt_QuadruplesDelay()
    {
        var subscription = new WebhookSubscription
        {
            BaseDelay = TimeSpan.FromSeconds(5),
            MaxDelay = TimeSpan.FromHours(1)
        };

        var delay = DeliveryService.CalculateBackoff(subscription, 3);

        Assert.Equal(TimeSpan.FromSeconds(20), delay);
    }

    [Fact]
    public void CalculateBackoff_ExceedsMaxDelay_CapsAtMax()
    {
        var subscription = new WebhookSubscription
        {
            BaseDelay = TimeSpan.FromSeconds(5),
            MaxDelay = TimeSpan.FromSeconds(60)
        };

        var delay = DeliveryService.CalculateBackoff(subscription, 10);

        Assert.Equal(TimeSpan.FromSeconds(60), delay);
    }

    [Fact]
    public void CalculateBackoff_FifthAttempt_StandardBackoff()
    {
        var subscription = new WebhookSubscription
        {
            BaseDelay = TimeSpan.FromSeconds(5),
            MaxDelay = TimeSpan.FromHours(1)
        };

        var delay = DeliveryService.CalculateBackoff(subscription, 5);

        Assert.Equal(TimeSpan.FromSeconds(80), delay); // 5 * 2^4 = 80
    }
}
