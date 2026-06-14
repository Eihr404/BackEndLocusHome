using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.DataAccess.Entities;

namespace Reservas.DataAccess.Configurations;

public sealed class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessageEntity>
{
    public void Configure(EntityTypeBuilder<ProcessedMessageEntity> builder)
    {
        builder.HasKey(x => new { x.MessageId, x.Consumer });
        builder.Property(x => x.MessageId)
            .HasColumnName("message_id")
            .HasMaxLength(64);
        builder.Property(x => x.Consumer)
            .HasColumnName("consumer")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired();
        builder.HasIndex(x => x.ProcessedAt);
    }
}
