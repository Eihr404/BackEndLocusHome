namespace Reservas.DataManagement.Models;

public class IdempotentRequestDataModel
{
    public string IdempotencyKey { get; set; } = string.Empty;

    public string OperationName { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int? ResourceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
