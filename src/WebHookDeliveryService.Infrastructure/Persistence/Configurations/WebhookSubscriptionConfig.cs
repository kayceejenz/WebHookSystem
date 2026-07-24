using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Infrastructure.Persistence.Configurations;

public class WebhookSubscriptionConfig : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable("webhook_subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Url).HasColumnName("url").IsRequired();
        builder.Property(x => x.Events).HasColumnName("events").HasColumnType("text[]");
        builder.Property(x => x.Secret).HasColumnName("secret").IsRequired();
        builder.Property(x => x.Active).HasColumnName("active").HasDefaultValue(true);
        builder.Property(x => x.MaxRetries).HasColumnName("max_retries").HasDefaultValue(5);
        builder.Property(x => x.BaseDelay).HasColumnName("base_delay").HasConversion<long>();
        builder.Property(x => x.MaxDelay).HasColumnName("max_delay").HasConversion<long>();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Active);
    }
}
