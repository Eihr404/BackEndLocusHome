using Microsoft.EntityFrameworkCore;
using Microservicio.Clientes.Business.DTOs.Maestros;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

public class MaestrosService : IMaestrosService
{
    private readonly IUnitOfWork _unitOfWork;

    public MaestrosService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CiudadDto>> ObtenerCiudadesAsync()
    {
        var entidades = await _unitOfWork.Ciudades.Query()
            .OrderBy(c => c.Nombre)
            .ToListAsync();
        return entidades.Select(MaestrosBusinessMapper.ToResponse).ToList();
    }

    public async Task<IEnumerable<PaisDto>> ObtenerPaisesAsync()
    {
        var entidades = await _unitOfWork.Paises.Query()
            .OrderBy(p => p.Nombre)
            .ToListAsync();
        return entidades.Select(MaestrosBusinessMapper.ToResponse).ToList();
    }

    public async Task<IEnumerable<MonedaDto>> ObtenerMonedasAsync()
    {
        var entidades = await _unitOfWork.Monedas.Query().ToListAsync();
        return entidades.Select(MaestrosBusinessMapper.ToResponse).ToList();
    }

    public async Task<IEnumerable<TipoAlojamientoDto>> ObtenerTiposAlojamientoAsync()
    {
        var entidades = await _unitOfWork.TiposAlojamiento.Query().ToListAsync();
        return entidades.Select(MaestrosBusinessMapper.ToResponse).ToList();
    }

    public async Task<IEnumerable<InstalacionDto>> ObtenerInstalacionesAsync()
    {
        var entidades = await _unitOfWork.Instalaciones.Query().ToListAsync();
        return entidades.Select(MaestrosBusinessMapper.ToResponse).ToList();
    }
}
