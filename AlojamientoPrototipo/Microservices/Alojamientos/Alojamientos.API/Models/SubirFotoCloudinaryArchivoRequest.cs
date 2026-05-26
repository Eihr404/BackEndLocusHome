using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Alojamientos.API.Models;

public class SubirFotoCloudinaryArchivoRequest
{
    [Required]
    public int AlojamientoId { get; set; }

    [Required]
    public IFormFile? Archivo { get; set; }

    public int Orden { get; set; } = 0;

    [MaxLength(200)]
    public string? Descripcion { get; set; }
}
