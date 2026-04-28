using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Repositories.Interfaces;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microservicio.Clientes.DataManagement.Mappers;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.DataManagement.Services;

/// <summary>
/// Servicio de datos de Cliente: orquesta los repositorios de la Capa 1
/// y convierte los resultados a DataModels seguros para la Capa 3 (Business).
/// </summary>
public class ClienteDataService : IClienteDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClienteDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClienteDataModel?> ObtenerPorIdAsync(int clienteId)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(clienteId);
        if (cliente == null) return null;

        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(cliente.UsuarioId);
        if (usuario == null) return null;

        return ClienteDataMapper.ToDataModel(cliente, usuario);
    }

    public async Task<ClienteDataModel?> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        var cliente = await _unitOfWork.Clientes.GetByUsuarioIdAsync(usuarioId);
        if (cliente == null) return null;

        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(cliente.UsuarioId);
        if (usuario == null) return null;

        return ClienteDataMapper.ToDataModel(cliente, usuario);
    }

    public async Task<DataPagedResult<ClienteDataModel>> BuscarAsync(ClienteFiltroDataModel filtro)
    {
        var paged = await _unitOfWork.Clientes.GetPaginadoAsync(filtro.PageNumber, filtro.PageSize);
        
        var items = new List<ClienteDataModel>();
        foreach (var cliente in paged.Items)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(cliente.UsuarioId);
            if (usuario != null) items.Add(ClienteDataMapper.ToDataModel(cliente, usuario));
        }

        return new DataPagedResult<ClienteDataModel>
        {
            Items        = items.AsReadOnly(),
            PageNumber   = paged.PageNumber,
            PageSize     = paged.PageSize,
            TotalRecords = paged.TotalCount
        };
    }

    public async Task<int> CrearAsync(ClienteDataModel modelo)
    {
        // 1. Obtener o asignar Rol 'Cliente' (ID 2 por defecto si no se encuentra)
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var rolCliente = roles.FirstOrDefault(r => r.Nombre == "Cliente");
        int rolId = rolCliente?.RolId ?? 2;

        // 2. Hashear la contraseña
        string passwordHash = "";
        if (!string.IsNullOrWhiteSpace(modelo.Password))
        {
            passwordHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(modelo.Password)));
        }

        // 3. Crear el Usuario
        var usuario = new Microservicio.Cliente.DatAccess.Entities.Seguridad.UsuarioEntity
        {
            NombreCompleto = modelo.NombreCompleto,
            Email = modelo.Email,
            PasswordHash = passwordHash,
            RolId = rolId,
            Estado = true,
            EmailVerificado = true,
            FechaCreacion = DateTime.UtcNow,
            EliminadoLogico = false
        };
        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        // 4. Crear el Cliente
        var entity = new ClienteEntity
        {
            UsuarioId        = usuario.UsuarioId,
            Telefono         = modelo.Telefono,
            FotoUrl          = modelo.FotoUrl,
            Domicilio        = modelo.Domicilio,
            FechaCreacion    = DateTime.UtcNow,
            EliminadoLogico  = false
        };
        await _unitOfWork.Clientes.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        
        return entity.ClienteId;
    }

    public async Task ActualizarAsync(ClienteDataModel modelo)
    {
        var entity = await _unitOfWork.Clientes.GetByIdAsync(modelo.ClienteId);
        if (entity == null) throw new KeyNotFoundException($"Cliente {modelo.ClienteId} no encontrado.");

        ClienteDataMapper.UpdateEntityFromModel(entity, modelo);
        await _unitOfWork.Clientes.UpdateAsync(entity);
    }

    public async Task EliminarAsync(int clienteId)
    {
        var entity = await _unitOfWork.Clientes.GetByIdAsync(clienteId);
        if (entity == null) throw new KeyNotFoundException($"Cliente {clienteId} no encontrado.");

        // Borrado lógico: no eliminamos de la BD, solo marcamos la bandera
        entity.EliminadoLogico  = true;
        entity.FechaModificacion = DateTime.UtcNow;
        await _unitOfWork.Clientes.UpdateAsync(entity);
    }
}
