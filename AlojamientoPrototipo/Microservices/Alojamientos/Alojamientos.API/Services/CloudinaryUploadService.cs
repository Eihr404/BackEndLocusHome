using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alojamientos.API.Services;

public interface ICloudinaryUploadService
{
    Task<string> UploadImageFromUrlAsync(string sourceUrl, CancellationToken cancellationToken = default);
}

public class CloudinaryUploadService : ICloudinaryUploadService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CloudinaryUploadService> _logger;
    private readonly CloudinaryCredentials _credentials;

    public CloudinaryUploadService(HttpClient httpClient, ILogger<CloudinaryUploadService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _credentials = CloudinaryCredentials.Parse(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
    }

    public async Task<string> UploadImageFromUrlAsync(string sourceUrl, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = BuildSignature(timestamp, _credentials.ApiSecret);
        var endpoint = $"https://api.cloudinary.com/v1_1/{_credentials.CloudName}/image/upload";

        using var content = new MultipartFormDataContent
        {
            { new StringContent(sourceUrl), "file" },
            { new StringContent(_credentials.ApiKey), "api_key" },
            { new StringContent(timestamp), "timestamp" },
            { new StringContent(signature), "signature" }
        };

        using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cloudinary upload failed with status {StatusCode}: {Payload}", response.StatusCode, payload);
            throw new InvalidOperationException("No se pudo subir la imagen a Cloudinary.");
        }

        var parsed = JsonSerializer.Deserialize<CloudinaryUploadResponse>(payload);
        if (string.IsNullOrWhiteSpace(parsed?.SecureUrl))
        {
            throw new InvalidOperationException("Cloudinary no devolvio una URL segura para la imagen.");
        }

        return parsed.SecureUrl;
    }

    private static string BuildSignature(string timestamp, string apiSecret)
    {
        var toSign = $"timestamp={timestamp}{apiSecret}";
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(toSign));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private sealed record CloudinaryUploadResponse
    {
        [JsonPropertyName("secure_url")]
        public string SecureUrl { get; init; } = string.Empty;
    }

    private sealed record CloudinaryCredentials(string CloudName, string ApiKey, string ApiSecret)
    {
        public static CloudinaryCredentials Parse(string? rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                throw new InvalidOperationException("La variable de entorno CLOUDINARY_URL no esta configurada.");
            }

            var uri = new Uri(rawUrl);
            var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.TrimEntries);
            if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
            {
                throw new InvalidOperationException("La variable CLOUDINARY_URL no tiene un formato valido.");
            }

            return new CloudinaryCredentials(uri.Host, userInfo[0], userInfo[1]);
        }
    }
}
