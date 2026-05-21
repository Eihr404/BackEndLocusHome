using Usuarios.Business.DTOs.Usuarios;
using Usuarios.Business.Interfaces;
using Usuarios.Business.Mappers;
using Usuarios.DataManagement.Interfaces;
using Usuarios.DataManagement.Models;

namespace Usuarios.Business.Services;

public class UsuariosService : IUsuariosService
{
    private readonly IUsuariosDataService _dataService;

    public UsuariosService(IUsuariosDataService dataService) => _dataService = dataService;

    public async Task<IEnumerable<UsuarioResponse>> GetAllAsync()
    {
        var models = await _dataService.GetAllAsync();
        return models.Select(UsuariosBusinessMapper.ToResponse);
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id)
    {
        var model = await _dataService.GetByIdAsync(id);
        return model != null ? UsuariosBusinessMapper.ToResponse(model) : null;
    }

    public async Task<UsuarioResponse> CrearUsuarioAsync(CrearUsuarioRequest request)
    {
        var existente = await _dataService.GetByEmailAsync(request.Email);

        if (existente != null)
        {
            throw new InvalidOperationException("Ya existe un usuario registrado con ese correo.");
        }

        var model = new UsuarioDataModel
        {
            Rol = string.IsNullOrWhiteSpace(request.Rol) ? "Cliente" : request.Rol,
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            NombreCompleto = request.NombreCompleto.Trim(),
            Estado = true,
            FechaCreacion = DateTime.UtcNow
        };

        var creado = await _dataService.CreateAsync(model);

        return UsuariosBusinessMapper.ToResponse(creado);
    }
}
