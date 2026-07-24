using Microsoft.EntityFrameworkCore;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Infrastructure.Persistence;

public class WebhookDbContext : DbContext
{
    public DbSet<WebhookSubscription> Subscriptions => Set<WebhookSubscription>();
    public DbSet<WebhookEvent> Events => Set<WebhookEvent>();
    public DbSet<WebhookDelivery> Deliveries => Set<WebhookDelivery>();
    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();
    public DbSet<DeadLetter> DeadLetters => Set<DeadLetter>();

    public WebhookDbContext(DbContextOptions<WebhookDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebhookDbContext).Assembly);
    }
}
