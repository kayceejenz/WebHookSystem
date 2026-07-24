using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Infrastructure.Persistence.Configurations;

public class DeliveryAttemptConfig : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("delivery_attempts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DeliveryId).HasColumnName("delivery_id");
        builder.Property(x => x.AttemptNumber).HasColumnName("attempt_number");
        builder.Property(x => x.ResponseCode).HasColumnName("response_code");
        builder.Property(x => x.ResponseBody).HasColumnName("response_body").HasColumnType("text");
        builder.Property(x => x.DurationMs).HasColumnName("duration_ms");
        builder.Property(x => x.SignatureSent).HasColumnName("signature_sent");
        builder.Property(x => x.Error).HasColumnName("error").HasColumnType("text");
        builder.Property(x => x.AttemptedAt).HasColumnName("attempted_at");

        builder.HasOne(x => x.Delivery)
            .WithMany(d => d.Attempts)
            .HasForeignKey(x => x.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.DeliveryId);
    }
}
