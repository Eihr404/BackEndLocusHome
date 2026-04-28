using System.ComponentModel.DataAnnotations;

namespace Microservicio.Clientes.Business.DTOs.Colaboradores;

public class ColaboradorDto
{
    public int ColaboradorId { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreEmpresa { get; set; }
    public string? Telefono { get; set; }
    public string? CuentaBancaria { get; set; }
    public bool Verificado { get; set; }
    public int PuntosAcumulados { get; set; }
}

public class CrearColaboradorDto
{
    [Required]
    public int UsuarioId { get; set; }

    [Required]
    [MaxLength(100)]
    public string NombreEmpresa { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;
}

public class ActualizarColaboradorDto
{
    [Required]
    [MaxLength(100)]
    public string NombreEmpresa { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CuentaBancaria { get; set; }
}
