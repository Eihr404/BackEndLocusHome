using Microsoft.EntityFrameworkCore;
using Microservicio.Clientes.Business.DTOs.Auth;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Microservicio.Clientes.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _config     = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException(["Email y contraseña son obligatorios."]);

        // 1. Buscar usuario por email
        var usuario = await _unitOfWork.Usuarios.GetByEmailAsync(request.Email)
            ?? throw new BusinessException("Credenciales inválidas.");

        // 2. Verificar contraseña (SHA256 → Base64)
        var passwordHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes(request.Password)));

        if (usuario.PasswordHash != passwordHash)
            throw new BusinessException("Credenciales inválidas.");

        if (!usuario.Estado)
            throw new BusinessException("La cuenta está inactiva.");

        // 3. Obtener rol directamente por RolId (FK directa en Usuarios)
        var rol = await _unitOfWork.Roles.GetByIdAsync(usuario.RolId);
        var nombreRol = rol?.Nombre ?? "Usuario";

        // 4. Generar JWT
        var jwtKey    = _config["Jwt:Key"] ?? "BookingProtoSecret2025!XyZ#Secure32";
        var jwtIssuer = _config["Jwt:Issuer"] ?? "BookingApi";
        var expiracion = DateTime.UtcNow.AddHours(
            _config.GetValue<int>("Jwt:ExpiresInHours", 8));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   usuario.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? ""),
            new(ClaimTypes.Name,               usuario.NombreCompleto ?? ""),
            new(ClaimTypes.Role,               nombreRol),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer, audience: jwtIssuer,
            claims: claims, expires: expiracion,
            signingCredentials: creds);

        // 5. Buscar o auto-crear el registro Cliente vinculado a este Usuario.
        //    Esto es necesario porque la tabla Reservas tiene FK a Clientes (no a Usuarios).
        var cliente = await _unitOfWork.Clientes.GetByUsuarioIdAsync(usuario.UsuarioId);
        if (cliente == null)
        {
            cliente = new Microservicio.Cliente.DatAccess.Entities.Core.ClienteEntity
            {
                UsuarioId       = usuario.UsuarioId,
                Calificacion    = 5.0m,
                TotalReservas   = 0,
                PuntosAcumulados = 0,
                UsuarioCreacion = usuario.Email ?? "SYSTEM"
            };
            await _unitOfWork.Clientes.AddAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
        }

        // 6. Buscar si el usuario es un Colaborador y tiene un registro
        int? colaboradorId = null;
        if (nombreRol == "Colaborador" || nombreRol == "Administrador")
        {
            // Optimizamos: buscamos directamente en la DB
            var registroColab = await _unitOfWork.Colaboradores.Query()
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.UsuarioId && !c.EliminadoLogico);

            if (registroColab != null)
            {
                colaboradorId = registroColab.ColaboradorId;
            }
            else if (nombreRol == "Colaborador")
            {
                // Auto-crear Colaborador si se le asignó el rol pero no tiene registro
                var nuevoColab = new Microservicio.Cliente.DatAccess.Entities.Seguridad.ColaboradorEntity
                {
                    UsuarioId = usuario.UsuarioId,
                    Verificado = false,
                    PuntosAcumulados = 0,
                    UsuarioCreacion = usuario.Email ?? "SYSTEM"
                };
                await _unitOfWork.Colaboradores.AddAsync(nuevoColab);
                await _unitOfWork.SaveChangesAsync();
                colaboradorId = nuevoColab.ColaboradorId;
            }
        }

        return new LoginResponse
        {
            Token          = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracion     = expiracion,
            NombreCompleto = usuario.NombreCompleto,
            Email          = usuario.Email,
            ClienteId      = cliente.ClienteId,
            ColaboradorId  = colaboradorId,
            Roles          = new List<string> { nombreRol }
        };
    }
}
