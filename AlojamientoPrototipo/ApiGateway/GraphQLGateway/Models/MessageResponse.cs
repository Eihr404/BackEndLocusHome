using System.Text.Json.Serialization;

namespace GraphQLGateway.Models;

public sealed record MessageResponse
{
    [JsonPropertyName("mensaje")]
    public string Mensaje { get; init; } = string.Empty;
}
