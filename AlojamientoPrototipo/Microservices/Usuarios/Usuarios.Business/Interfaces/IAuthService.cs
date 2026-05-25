using Usuarios.Business.DTOs.Auth;

namespace Usuarios.Business.Interfaces;

public interface IAuthService
{
    Task<LoginAttemptResult> LoginAsync(LoginRequest request);
}
