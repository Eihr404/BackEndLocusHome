using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microservicio.Clientes.Business.DTOs.Pagos;

namespace Microservicio.Clientes.Business.Mappers;

public static class PagosBusinessMapper
{
    public static PagoDto ToResponse(PagoEntity entity) => new PagoDto
    {
        PagoId = entity.PagoId,
        ReservaId = entity.ReservaId,
        Monto = entity.Monto,
        // Moneda se omite o se asigna en el servicio si se cruza con otra tabla
        ReferenciaPago = entity.ReferenciaPago,
        TipoPago = entity.TipoPago,
        Estado = entity.Estado,
        FechaPago = entity.FechaPago
    };

    public static PagoEntity ToDataModel(ProcesarPagoDto dto) => new PagoEntity
    {
        ReservaId = dto.ReservaId,
        MetodoPagoId = null,  // Always null to avoid FK_Pagos_MetodosPago violation
        Monto = dto.Monto,
        MonedaId = dto.MonedaId > 0 ? dto.MonedaId : 1,
        TipoPago = dto.TipoPago ?? "Tarjeta",
        ReferenciaPago = dto.ReferenciaPago,
        Estado = "Completado",
        FechaPago = DateTime.UtcNow,
        UsuarioCreacion = "System"
    };
}
