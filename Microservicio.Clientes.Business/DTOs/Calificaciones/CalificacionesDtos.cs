using System.ComponentModel.DataAnnotations;

namespace Microservicio.Clientes.Business.DTOs.Calificaciones;

public class CalificacionHotelDto
{
    public int CalificacionId { get; set; }
    public int PropiedadId { get; set; }
    public int ClienteId { get; set; }
    public int ReservaId { get; set; }
    public decimal Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CrearCalificacionHotelDto
{
    [Required]
    public int PropiedadId { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int ReservaId { get; set; }

    [Required]
    [Range(1.0, 5.0, ErrorMessage = "La puntuación debe estar entre 1.0 y 5.0")]
    public decimal Puntuacion { get; set; }

    [MaxLength(500)]
    public string? Comentario { get; set; }
}
