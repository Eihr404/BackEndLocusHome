using System.ComponentModel.DataAnnotations;

namespace Microservicio.Clientes.Business.DTOs.Habitaciones;

public class HabitacionDto
{
    public int HabitacionId { get; set; }
    public int PropiedadId { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int CapacidadAdultos { get; set; }
    public int CapacidadNinos { get; set; }
    public int NumBanos { get; set; }
    public int NumDormitorios { get; set; }
    public bool AdmiteMascotas { get; set; }
    public bool TieneCocina { get; set; }
    public bool TieneAireAcondicionado { get; set; }
    public decimal? SuperficieM2 { get; set; }
    public bool Estado { get; set; }
}

public class CrearHabitacionDto
{
    [Required]
    public int PropiedadId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public int CapacidadAdultos { get; set; }

    [Required]
    public int NumBanos { get; set; }

    public int CapacidadNinos { get; set; }
    public int NumDormitorios { get; set; }
    public string? Descripcion { get; set; }
    public bool AdmiteMascotas { get; set; }
    public bool TieneCocina { get; set; }
    public bool TieneAireAcondicionado { get; set; }
    public decimal? SuperficieM2 { get; set; }
}

public class ActualizarHabitacionDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public int CapacidadAdultos { get; set; }

    [Required]
    public int NumBanos { get; set; }

    public int CapacidadNinos { get; set; }
    public int NumDormitorios { get; set; }
    public string? Descripcion { get; set; }
    public bool AdmiteMascotas { get; set; }
    public bool TieneCocina { get; set; }
    public bool TieneAireAcondicionado { get; set; }
    public decimal? SuperficieM2 { get; set; }
}
