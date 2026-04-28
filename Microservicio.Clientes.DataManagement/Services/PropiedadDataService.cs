using Microsoft.EntityFrameworkCore;
using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microservicio.Clientes.DataManagement.Mappers;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.DataManagement.Services;

/// <summary>
/// Servicio de datos de Propiedad: búsqueda y consulta de alojamientos.
/// </summary>
public class PropiedadDataService : IPropiedadDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public PropiedadDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PropiedadDataModel?> ObtenerPorIdAsync(int propiedadId)
    {
        var entity = await _unitOfWork.Propiedades.GetByIdAsync(propiedadId);
        if (entity == null) return null;

        var model = PropiedadDataMapper.ToDataModel(entity);
        var ciudad = await _unitOfWork.Ciudades.GetByIdAsync(entity.CiudadId);
        if (ciudad != null) model.Ciudad = ciudad.Nombre;
        
        return model;
    }

    public async Task<DataPagedResult<PropiedadDataModel>> BuscarAsync(PropiedadFiltroDataModel filtro)
    {
        // 1. Iniciar consulta base
        var query = _unitOfWork.Propiedades.Query()
            .Where(p => p.Estado == "Activa" && !p.EliminadoLogico);

        // 2. Aplicar filtros básicos (Server-side)
        if (filtro.CiudadId.HasValue)
            query = query.Where(p => p.CiudadId == filtro.CiudadId);
        
        if (filtro.AdmiteMascotas.HasValue)
            query = query.Where(p => p.AdmiteMascotas == filtro.AdmiteMascotas);
        
        if (filtro.EstrellasMinimas.HasValue)
            query = query.Where(p => p.Estrellas >= filtro.EstrellasMinimas);

        // 3. Filtro de disponibilidad y capacidad (Server-side subquery)
        if (filtro.FechaCheckIn.HasValue && filtro.FechaCheckOut.HasValue)
        {
            var checkIn = filtro.FechaCheckIn.Value.Date;
            var checkOut = filtro.FechaCheckOut.Value.Date;

            // Buscamos propiedades que tengan al menos una habitación que:
            // - Tenga capacidad suficiente
            // - No tenga registros de "No Disponible" en ese rango de fechas
            query = query.Where(p => _unitOfWork.Habitaciones.Query()
                .Any(h => h.PropiedadId == p.PropiedadId && !h.EliminadoLogico &&
                          h.CapacidadAdultos >= filtro.NumAdultos && h.CapacidadNinos >= filtro.NumNinos &&
                          !_unitOfWork.Disponibilidades.Query()
                            .Any(d => d.HabitacionId == h.HabitacionId && 
                                      d.Fecha >= checkIn && d.Fecha < checkOut && !d.Disponible)));
        }
        else
        {
            // Si no hay fechas, al menos que tenga habitaciones con capacidad
            query = query.Where(p => _unitOfWork.Habitaciones.Query()
                .Any(h => h.PropiedadId == p.PropiedadId && !h.EliminadoLogico &&
                          h.CapacidadAdultos >= filtro.NumAdultos && h.CapacidadNinos >= filtro.NumNinos));
        }

        // 4. Ordenación y Totales
        var totalRecords = await query.CountAsync();
        
        var pagedEntities = await query
            .OrderByDescending(p => p.CalificacionPromedio)
            .Skip((filtro.PageNumber - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync();

        // 5. Cargar nombres de ciudades para el resultado final (solo las necesarias)
        var ciudadIds = pagedEntities.Select(p => p.CiudadId).Distinct().ToList();
        var ciudadesDict = (await _unitOfWork.Ciudades.Query()
            .Where(c => ciudadIds.Contains(c.CiudadId))
            .ToListAsync())
            .ToDictionary(c => c.CiudadId, c => c.Nombre);

        // 6. Mapeo final
        var items = pagedEntities.Select(p => 
        {
            var model = PropiedadDataMapper.ToDataModel(p);
            if (ciudadesDict.TryGetValue(p.CiudadId, out var nombreCiudad))
                model.Ciudad = nombreCiudad;
            return model;
        }).ToList();

        return new DataPagedResult<PropiedadDataModel>
        {
            Items        = items.AsReadOnly(),
            PageNumber   = filtro.PageNumber,
            PageSize     = filtro.PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<IReadOnlyCollection<PropiedadDataModel>> ObtenerPorColaboradorAsync(int colaboradorId)
    {
        var todos = await _unitOfWork.Propiedades.GetAllAsync();
        return todos
            .Where(p => p.ColaboradorId == colaboradorId && !p.EliminadoLogico)
            .Select(PropiedadDataMapper.ToDataModel)
            .ToList()
            .AsReadOnly();
    }
}
