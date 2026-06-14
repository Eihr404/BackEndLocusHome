using System.ComponentModel.DataAnnotations.Schema;

namespace Reservas.DataAccess.Entities;

[Table("processed_messages")]
public class ProcessedMessageEntity
{
    public string MessageId { get; set; } = string.Empty;

    public string Consumer { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
