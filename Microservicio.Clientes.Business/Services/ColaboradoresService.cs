using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Clientes.Business.DTOs.Colaboradores;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

public class ColaboradoresService : IColaboradoresService
{
    private readonly IUnitOfWork _unitOfWork;

    public ColaboradoresService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ColaboradorDto>> ObtenerTodosAsync()
    {
        var entidades = await _unitOfWork.Colaboradores.GetAllAsync();
        return entidades.Where(e => !e.EliminadoLogico)
                        .Select(ColaboradoresBusinessMapper.ToResponse)
                        .ToList();
    }

    public async Task<ColaboradorDto> ObtenerPorIdAsync(int id)
    {
        var entidad = await _unitOfWork.Colaboradores.GetByIdAsync(id) 
            ?? throw new NotFoundException("Colaborador", id);

        if (entidad.EliminadoLogico)
            throw new NotFoundException("Colaborador", id);

        return ColaboradoresBusinessMapper.ToResponse(entidad);
    }

    public async Task<ColaboradorDto> CrearAsync(CrearColaboradorDto dto)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(dto.UsuarioId)
            ?? throw new BusinessException("El UsuarioId especificado no existe.");

        var entidad = ColaboradoresBusinessMapper.ToDataModel(dto);
        entidad.Verificado = false; // Por defecto no verificado

        await _unitOfWork.Colaboradores.AddAsync(entidad);
        await _unitOfWork.SaveChangesAsync();

        return ColaboradoresBusinessMapper.ToResponse(entidad);
    }

    public async Task<ColaboradorDto> ActualizarAsync(int id, ActualizarColaboradorDto dto)
    {
        var entidad = await _unitOfWork.Colaboradores.GetByIdAsync(id)
            ?? throw new NotFoundException("Colaborador", id);

        if (entidad.EliminadoLogico)
            throw new NotFoundException("Colaborador", id);

        entidad.NombreEmpresa = dto.NombreEmpresa;
        entidad.Telefono = dto.Telefono;
        entidad.CuentaBancaria = dto.CuentaBancaria;
        entidad.FechaModificacion = DateTime.UtcNow;

        await _unitOfWork.Colaboradores.UpdateAsync(entidad);
        await _unitOfWork.SaveChangesAsync();

        return ColaboradoresBusinessMapper.ToResponse(entidad);
    }

    public async Task EliminarAsync(int id)
    {
        var entidad = await _unitOfWork.Colaboradores.GetByIdAsync(id)
            ?? throw new NotFoundException("Colaborador", id);

        // Eliminado Lógico
        entidad.EliminadoLogico = true;
        entidad.FechaModificacion = DateTime.UtcNow;

        await _unitOfWork.Colaboradores.UpdateAsync(entidad);
        await _unitOfWork.SaveChangesAsync();
    }
}
