using Microservicio.Clientes.Business.DTOs.Pagos;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.Business.Services;

public class PagosService : IPagosService
{
    private readonly IUnitOfWork _unitOfWork;

    public PagosService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PagoDto>> ObtenerPorClienteAsync(int clienteId)
    {
        // Corregido: PagoEntity no tiene ClienteId, hay que unirlo con Reservas
        var query = from pago in _unitOfWork.Pagos.Query()
                    join reserva in _unitOfWork.Reservas.Query() on pago.ReservaId equals reserva.ReservaId
                    where reserva.ClienteId == clienteId && !pago.EliminadoLogico
                    orderby pago.FechaPago descending
                    select pago;

        var entities = await query.ToListAsync();

        return entities.Select(PagosBusinessMapper.ToResponse).ToList();
    }

    public async Task<IEnumerable<PagoDto>> ObtenerPagosPorReservaAsync(int reservaId)
    {
        var entities = await _unitOfWork.Pagos.Query()
            .Where(p => p.ReservaId == reservaId && !p.EliminadoLogico)
            .ToListAsync();

        return entities.Select(PagosBusinessMapper.ToResponse).ToList();
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
