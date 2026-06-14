using System.ComponentModel.DataAnnotations.Schema;

namespace Reservas.DataAccess.Entities;

[Table("idempotent_requests")]
public class IdempotentRequestEntity
{
    public string IdempotencyKey { get; set; } = string.Empty;

    public string OperationName { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int? ResourceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
