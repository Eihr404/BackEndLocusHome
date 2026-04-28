using Microservicio.Clientes.Business.DTOs.Pagos;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

public class PagosService : IPagosService
{
    private readonly IUnitOfWork _unitOfWork;

    public PagosService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PagoDto>> ObtenerPagosPorReservaAsync(int reservaId)
    {
        var pagos = await _unitOfWork.Pagos.GetAllAsync();
        var pagosDeReserva = pagos.Where(p => p.ReservaId == reservaId);

        return pagosDeReserva.Select(PagosBusinessMapper.ToResponse).ToList();
    }

    public async Task<PagoDto> ProcesarPagoAsync(ProcesarPagoDto dto)
    {
        // 1. Validar que la Reserva Exista
        var reserva = await _unitOfWork.Reservas.GetByIdAsync(dto.ReservaId)
            ?? throw new NotFoundException("Reserva", dto.ReservaId);

        if (reserva.Estado == "Cancelada" || reserva.Estado == "Rechazada")
            throw new BusinessException($"No se puede procesar el pago para una reserva con estado {reserva.Estado}.");

        // 2. Crear la entidad de Pago.
        //    MetodoPagoId se deja en NULL para evitar la FK a MetodosPagoCliente
        //    (la columna Pagos.MetodoPagoId es INT NULL en la BD).
        //    El campo TipoPago (NVARCHAR) registra el tipo seleccionado ("Tarjeta", "EnSitio").
        var pagoEntity = new Microservicio.Cliente.DatAccess.Entities.Reservas.PagoEntity
        {
            ReservaId      = dto.ReservaId,
            MetodoPagoId   = null,            // Evita FK violation
            Monto          = dto.Monto,
            MonedaId       = dto.MonedaId > 0 ? dto.MonedaId : 1,
            TipoPago       = dto.TipoPago ?? "Tarjeta",
            ReferenciaPago = dto.ReferenciaPago ?? $"BP-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            Estado         = "Completado",
            FechaPago      = DateTime.UtcNow,
            UsuarioCreacion = reserva.ClienteId.ToString()
        };
        await _unitOfWork.Pagos.AddAsync(pagoEntity);

        // 3. Actualizar estado de la reserva a "Confirmada"
        if (string.IsNullOrEmpty(reserva.Estado) || reserva.Estado == "Pendiente")
        {
            reserva.Estado = "Confirmada";
            reserva.FechaModificacion = DateTime.UtcNow;
            await _unitOfWork.Reservas.UpdateAsync(reserva);
        }

        await _unitOfWork.SaveChangesAsync();

        return PagosBusinessMapper.ToResponse(pagoEntity);
    }
}
