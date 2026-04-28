using Microsoft.EntityFrameworkCore;
using Microservicio.Clientes.Business.DTOs.Calificaciones;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

public class CalificacionesService : ICalificacionesService
{
    private readonly IUnitOfWork _unitOfWork;

    public CalificacionesService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CalificacionHotelDto>> ObtenerPorPropiedadAsync(int propiedadId)
    {
        var entities = await _unitOfWork.CalificacionesHotel.Query()
            .Where(c => c.PropiedadId == propiedadId)
            .ToListAsync();

        return entities.Select(CalificacionesBusinessMapper.ToResponse).ToList();
    }

    public async Task<CalificacionHotelDto> AgregarCalificacionAsync(CrearCalificacionHotelDto dto)
    {
        // 1. Validar Propiedad
        var propiedad = await _unitOfWork.Propiedades.GetByIdAsync(dto.PropiedadId)
            ?? throw new NotFoundException("Propiedad", dto.PropiedadId);

        // 2. Validar Cliente
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId)
            ?? throw new NotFoundException("Cliente", dto.ClienteId);

        // 3. Validar Reserva
        var reserva = await _unitOfWork.Reservas.GetByIdAsync(dto.ReservaId)
            ?? throw new NotFoundException("Reserva", dto.ReservaId);

        // Solo se puede calificar si la reserva es de ese cliente, para esa propiedad
        if (reserva.ClienteId != dto.ClienteId || reserva.PropiedadId != dto.PropiedadId)
            throw new BusinessException("La reserva no coincide con el cliente o la propiedad especificada.");

        var entidad = CalificacionesBusinessMapper.ToDataModel(dto);
        await _unitOfWork.CalificacionesHotel.AddAsync(entidad);

        // Recalcular promedio de calificación de la Propiedad (Server-side)
        var totalCalificaciones = await _unitOfWork.CalificacionesHotel.Query()
            .Where(c => c.PropiedadId == dto.PropiedadId)
            .CountAsync();
        
        var sumaPuntuaciones = await _unitOfWork.CalificacionesHotel.Query()
            .Where(c => c.PropiedadId == dto.PropiedadId)
            .SumAsync(c => c.Puntuacion);

        propiedad.CalificacionPromedio = (sumaPuntuaciones + dto.Puntuacion) / (totalCalificaciones + 1);
        propiedad.TotalResenas = totalCalificaciones + 1;
        
        await _unitOfWork.Propiedades.UpdateAsync(propiedad);
        await _unitOfWork.SaveChangesAsync();

        return CalificacionesBusinessMapper.ToResponse(entidad);
    }
}
