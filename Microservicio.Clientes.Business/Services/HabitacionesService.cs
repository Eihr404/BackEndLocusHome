using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Clientes.Business.DTOs.Habitaciones;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

public class HabitacionesService : IHabitacionesService
{
    private readonly IUnitOfWork _unitOfWork;

    public HabitacionesService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HabitacionDto>> ObtenerPorPropiedadAsync(int propiedadId)
    {
        // Validamos si la propiedad existe
        var propiedad = await _unitOfWork.Propiedades.GetByIdAsync(propiedadId)
            ?? throw new NotFoundException("Propiedad", propiedadId);

        var entidades = await _unitOfWork.Habitaciones.GetAllAsync();
        var habitacionDePropiedad = entidades.Where(h => h.PropiedadId == propiedadId && !h.EliminadoLogico);

        return habitacionDePropiedad.Select(HabitacionesBusinessMapper.ToResponse).ToList();
    }

    public async Task<HabitacionDto> ObtenerPorIdAsync(int id)
    {
        var entidad = await _unitOfWork.Habitaciones.GetByIdAsync(id)
            ?? throw new NotFoundException("Habitacion", id);

        if (entidad.EliminadoLogico)
            throw new NotFoundException("Habitacion", id);

        return HabitacionesBusinessMapper.ToResponse(entidad);
    }

    public async Task<HabitacionDto> CrearAsync(CrearHabitacionDto dto)
    {
        var propiedad = await _unitOfWork.Propiedades.GetByIdAsync(dto.PropiedadId)
            ?? throw new BusinessException("La PropiedadId especificada no existe.");

        var entidad = HabitacionesBusinessMapper.ToDataModel(dto);
        entidad.Estado = true; // Habitacion disponible por defecto al crearse

        await _unitOfWork.Habitaciones.AddAsync(entidad);
        await _unitOfWork.SaveChangesAsync();

        return HabitacionesBusinessMapper.ToResponse(entidad);
    }

    public async Task<HabitacionDto> ActualizarAsync(int id, ActualizarHabitacionDto dto)
    {
        var entidad = await _unitOfWork.Habitaciones.GetByIdAsync(id)
            ?? throw new NotFoundException("Habitacion", id);

        if (entidad.EliminadoLogico)
            throw new NotFoundException("Habitacion", id);

        entidad.Nombre = dto.Nombre;
        entidad.CapacidadAdultos = dto.CapacidadAdultos;
        entidad.NumBanos = dto.NumBanos;
        entidad.CapacidadNinos = dto.CapacidadNinos;
        entidad.NumDormitorios = dto.NumDormitorios;
        entidad.Descripcion = dto.Descripcion;
        entidad.AdmiteMascotas = dto.AdmiteMascotas;
        entidad.TieneCocina = dto.TieneCocina;
        entidad.TieneAireAcondicionado = dto.TieneAireAcondicionado;
        entidad.SuperficieM2 = dto.SuperficieM2;
        entidad.FechaModificacion = DateTime.UtcNow;

        await _unitOfWork.Habitaciones.UpdateAsync(entidad);
        await _unitOfWork.SaveChangesAsync();

        return HabitacionesBusinessMapper.ToResponse(entidad);
    }

    public async Task EliminarAsync(int id)
    {
        var entidad = await _unitOfWork.Habitaciones.GetByIdAsync(id)
            ?? throw new NotFoundException("Habitacion", id);

        entidad.EliminadoLogico = true;
        entidad.FechaModificacion = DateTime.UtcNow;

        await _unitOfWork.Habitaciones.UpdateAsync(entidad);
        await _unitOfWork.SaveChangesAsync();
    }
}
