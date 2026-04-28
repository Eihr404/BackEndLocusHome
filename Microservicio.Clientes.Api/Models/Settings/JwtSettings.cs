namespace Microservicio.Clientes.Api.Models.Settings;

/// <summary>Configuración fuertemente tipada para JWT leída desde appsettings.json.</summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int ExpiresInHours { get; set; } = 8;
}
