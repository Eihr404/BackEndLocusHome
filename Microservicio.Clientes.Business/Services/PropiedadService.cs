using Microservicio.Clientes.Business.DTOs.Propiedades;
using Microservicio.Clientes.Business.DTOs.Shared;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.Business.Validators;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.Business.Services;

public class PropiedadService : IPropiedadService
{
    private readonly IPropiedadDataService _dataService;
    private readonly IUnitOfWork _unitOfWork;

    public PropiedadService(IPropiedadDataService dataService, IUnitOfWork unitOfWork)
    {
        _dataService = dataService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PropiedadResponse> ObtenerPorIdAsync(int propiedadId)
    {
        var model = await _dataService.ObtenerPorIdAsync(propiedadId)
            ?? throw new NotFoundException("Propiedad", propiedadId);
        return PropiedadBusinessMapper.ToResponse(model);
    }

    public async Task<PagedResponse<PropiedadTarjetaResponse>> BuscarAsync(BuscarPropiedadRequest request)
    {
        var filtro = new PropiedadFiltroDataModel
        {
            CiudadId         = request.CiudadId,
            AdmiteMascotas   = request.AdmiteMascotas,
            EstrellasMinimas = request.EstrellasMinimas,
            PrecioMaximo     = request.PrecioMaximo,
            NumAdultos       = request.NumAdultos,
            NumNinos         = request.NumNinos,
            FechaCheckIn     = request.FechaCheckIn,
            FechaCheckOut    = request.FechaCheckOut,
            PageNumber       = request.PageNumber,
            PageSize         = request.PageSize
        };
        var paged = await _dataService.BuscarAsync(filtro);
        return PropiedadBusinessMapper.ToPagedResponse(paged);
    }

    public async Task<PropiedadResponse> CrearAsync(CrearPropiedadRequest request)
    {
        PropiedadValidator.Validar(request);
        
        var entity = PropiedadBusinessMapper.ToEntity(request);
        await _unitOfWork.Propiedades.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return await ObtenerPorIdAsync(entity.PropiedadId);
    }

    public async Task<IReadOnlyCollection<PropiedadResponse>> ObtenerPorColaboradorAsync(int colaboradorId)
    {
        var lista = await _dataService.ObtenerPorColaboradorAsync(colaboradorId);
        return lista.Select(PropiedadBusinessMapper.ToResponse).ToList().AsReadOnly();
    }

    public async Task CambiarEstadoAsync(CambiarEstadoPropiedadRequest request)
    {
        var estadosValidos = new[] { "Activa", "Inactiva", "EnMantenimiento" };
        if (!estadosValidos.Contains(request.NuevoEstado))
            throw new ValidationException([$"Estado inválido. Use: {string.Join(", ", estadosValidos)}"]);

        var propiedad = await _unitOfWork.Propiedades.GetByIdAsync(request.PropiedadId)
            ?? throw new NotFoundException("Propiedad", request.PropiedadId);

        propiedad.Estado = request.NuevoEstado;
        propiedad.FechaModificacion = DateTime.UtcNow;
        propiedad.UsuarioModificacion = "System";

        await _unitOfWork.Propiedades.UpdateAsync(propiedad);
        await _unitOfWork.SaveChangesAsync();
    }
}
