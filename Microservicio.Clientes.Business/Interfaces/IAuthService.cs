using Microservicio.Clientes.Business.DTOs.Auth;

namespace Microservicio.Clientes.Business.Interfaces;

/// <summary>Contrato del servicio de autenticación JWT.</summary>
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
