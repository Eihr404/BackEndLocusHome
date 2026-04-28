using System.ComponentModel.DataAnnotations;

namespace Microservicio.Clientes.Business.DTOs.Pagos;

public class PagoDto
{
    public int PagoId { get; set; }
    public int ReservaId { get; set; }
    public decimal Monto { get; set; }
    public string? Moneda { get; set; }
    public string? ReferenciaPago { get; set; }
    public string? TipoPago { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaPago { get; set; }
}

public class ProcesarPagoDto
{
    [Required]
    public int ReservaId { get; set; }

    public int? MetodoPagoId { get; set; }  // Nullable — matches DB column INT NULL

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal Monto { get; set; }

    [Required]
    public int MonedaId { get; set; }

    public string? TipoPago { get; set; } // Ejemplo: "Tarjeta", "Transferencia"
    public string? ReferenciaPago { get; set; }
}
