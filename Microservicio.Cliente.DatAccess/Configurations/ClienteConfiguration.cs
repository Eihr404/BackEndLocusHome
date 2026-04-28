using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microservicio.Cliente.DatAccess.Entities.Core;

namespace Microservicio.Cliente.DatAccess.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<ClienteEntity>
    {
        public void Configure(EntityTypeBuilder<ClienteEntity> builder)
        {
            // Especificaciones exactas para que EF Core entienda los tipos en SQL
            builder.Property(c => c.Calificacion)
                   .HasColumnType("decimal(3,2)");

            // La auditoría se hereda automáticamente de BaseEntity
            builder.Property(c => c.FechaCreacion)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
