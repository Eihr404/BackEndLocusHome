using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Facturacion.DataAccess.Entities;

namespace Facturacion.DataAccess.Configurations;

public sealed class IdempotentRequestConfiguration : IEntityTypeConfiguration<IdempotentRequestEntity>
{
    public void Configure(EntityTypeBuilder<IdempotentRequestEntity> builder)
    {
        builder.ToTable("idempotent_requests");
        builder.HasKey(x => new { x.IdempotencyKey, x.OperationName });

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(128);
        builder.Property(x => x.OperationName)
            .HasColumnName("operation_name")
            .HasMaxLength(64);
        builder.Property(x => x.RequestHash)
            .HasColumnName("request_hash")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.ResourceId)
            .HasColumnName("resource_id");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ResourceId);
    }
}
