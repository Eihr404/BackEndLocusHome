using Microservicio.Clientes.DataManagement.Interfaces;
using Microservicio.Clientes.DataManagement.Mappers;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.DataManagement.Services;

/// <summary>
/// Servicio de datos de Reservas: consulta, creación y cambio de estado.
/// </summary>
public class ReservaDataService : IReservaDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReservaDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservaDataModel?> ObtenerPorCodigoAsync(string codigo)
    {
        var todos = await _unitOfWork.Reservas.GetAllAsync();
        var entity = todos.FirstOrDefault(r => r.CodigoReserva == codigo && !r.EliminadoLogico);
        return entity == null ? null : ReservaDataMapper.ToDataModel(entity);
    }

    public async Task<IReadOnlyCollection<ReservaDataModel>> ObtenerTodasAsync()
    {
        var todos = await _unitOfWork.Reservas.GetAllAsync();
        return todos
            .Where(r => !r.EliminadoLogico)
            .OrderByDescending(r => r.FechaCreacion)
            .Select(ReservaDataMapper.ToDataModel)
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<ReservaDataModel>> ObtenerPorClienteAsync(int clienteId)
    {
        var todos = await _unitOfWork.Reservas.GetAllAsync();
        return todos
            .Where(r => r.ClienteId == clienteId && !r.EliminadoLogico)
            .OrderByDescending(r => r.FechaCreacion)
            .Select(ReservaDataMapper.ToDataModel)
            .ToList()
            .AsReadOnly();
    }

    public async Task<ReservaDataModel> CrearReservaAsync(CrearReservaDataModel modelo)
    {
        // Auto-Seed de Tarifas: si la habitación no tiene precio, asignar $150 USD por defecto
        foreach (var habId in modelo.HabitacionIds)
        {
            var tieneTarifa = (await _unitOfWork.Tarifas.GetAllAsync())
                .Any(t => t.HabitacionId == habId && t.Estado);
            if (!tieneTarifa)
            {
                await _unitOfWork.Tarifas.AddAsync(new Microservicio.Cliente.DatAccess.Entities.Core.TarifaEntity
                {
                    HabitacionId   = habId,
                    MonedaId       = 1,
                    PrecioPorNoche = 150.00m,
                    FechaInicio    = new DateTime(2020, 1, 1),
                    FechaFin       = new DateTime(2030, 12, 31),
                    Estado         = true,
                    UsuarioCreacion = "SYSTEM"
                });
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // Calcular totales con EF Core (sin stored procedure — compatible PostgreSQL)
        var todasTarifas = await _unitOfWork.Tarifas.GetAllAsync();
        int numNoches = (modelo.FechaCheckOut - modelo.FechaCheckIn).Days;

        decimal total = todasTarifas
            .Where(t => modelo.HabitacionIds.Contains(t.HabitacionId) && t.Estado)
            .Sum(t => t.PrecioPorNoche * numNoches);

        if (total == 0) total = 150m * numNoches * modelo.HabitacionIds.Count;

        // Generar código de reserva único
        string codigoReserva = $"BK{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        // Crear la reserva
        var reservaEntity = new Microservicio.Cliente.DatAccess.Entities.Reservas.ReservaEntity
        {
            ClienteId       = modelo.ClienteId,
            PropiedadId     = modelo.PropiedadId,
            FechaCheckIn    = modelo.FechaCheckIn,
            FechaCheckOut   = modelo.FechaCheckOut,
            NumAdultos      = modelo.NumAdultos,
            NumNinos        = modelo.NumNinos,
            LlevaMascotas   = modelo.LlevaMascotas,
            NumHabitaciones = modelo.HabitacionIds.Count,
            MonedaId        = 1,
            SubTotal        = total,
            Total           = total,
            CodigoReserva   = codigoReserva,
            Estado          = "Pendiente",
            UsuarioCreacion = modelo.ClienteId.ToString()
        };

        await _unitOfWork.Reservas.AddAsync(reservaEntity);
        await _unitOfWork.SaveChangesAsync();

        // Insertar detalle por habitación
        foreach (var habId in modelo.HabitacionIds)
        {
            var tarifa = todasTarifas.FirstOrDefault(t => t.HabitacionId == habId && t.Estado);
            decimal precio = tarifa?.PrecioPorNoche ?? 150m;
            await _unitOfWork.ReservaDetalles.AddAsync(
                new Microservicio.Cliente.DatAccess.Entities.Reservas.ReservaDetalleHabitacionEntity
                {
                    ReservaId           = reservaEntity.ReservaId,
                    HabitacionId        = habId,
                    PrecioPorNoche      = precio,
                    NumNoches           = numNoches,
                    SubTotalHabitacion  = precio * numNoches
                });
        }

        // Actualizar contador del cliente
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(modelo.ClienteId);
        if (cliente != null)
        {
            cliente.TotalReservas++;
            await _unitOfWork.Clientes.UpdateAsync(cliente);
        }

        await _unitOfWork.SaveChangesAsync();

        var entity = await _unitOfWork.Reservas.GetByIdAsync(reservaEntity.ReservaId);
        return ReservaDataMapper.ToDataModel(entity!);
    }

    public async Task CambiarEstadoAsync(int reservaId, string nuevoEstado)
    {
        var entity = await _unitOfWork.Reservas.GetByIdAsync(reservaId)
            ?? throw new KeyNotFoundException($"Reserva {reservaId} no encontrada.");

        entity.Estado             = nuevoEstado;
        entity.FechaModificacion  = DateTime.UtcNow;
        await _unitOfWork.Reservas.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
