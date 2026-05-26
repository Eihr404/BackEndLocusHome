using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alojamientos.API.Services;

public interface ICloudinaryUploadService
{
    Task<string> UploadImageFromUrlAsync(string sourceUrl, CancellationToken cancellationToken = default);
    Task<string> UploadImageAsync(Stream stream, string fileName, string? contentType, CancellationToken cancellationToken = default);
}

public class CloudinaryUploadService : ICloudinaryUploadService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CloudinaryUploadService> _logger;
    private readonly CloudinaryCredentials _credentials;
    private readonly string? _uploadPreset;

    public CloudinaryUploadService(HttpClient httpClient, ILogger<CloudinaryUploadService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _credentials = CloudinaryCredentials.Parse(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
        _uploadPreset = NormalizeOptionalEnvironmentValue(Environment.GetEnvironmentVariable("CLOUDINARY_UPLOAD_PRESET"));
    }

    public async Task<string> UploadImageFromUrlAsync(string sourceUrl, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var endpoint = $"https://api.cloudinary.com/v1_1/{_credentials.CloudName}/image/upload";

        using var content = new MultipartFormDataContent
        {
            { new StringContent(sourceUrl), "file" }
        };

        AddCloudinaryAuthFields(content, timestamp);

        using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cloudinary upload failed with status {StatusCode}: {Payload}", response.StatusCode, payload);
            throw new InvalidOperationException($"Cloudinary rechazo la imagen: {payload}");
        }

        var parsed = JsonSerializer.Deserialize<CloudinaryUploadResponse>(payload);
        if (string.IsNullOrWhiteSpace(parsed?.SecureUrl))
        {
            throw new InvalidOperationException("Cloudinary no devolvio una URL segura para la imagen.");
        }

        return parsed.SecureUrl;
    }

    public async Task<string> UploadImageAsync(
        Stream stream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var endpoint = $"https://api.cloudinary.com/v1_1/{_credentials.CloudName}/image/upload";

        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

        using var content = new MultipartFormDataContent
        {
            { fileContent, "file", string.IsNullOrWhiteSpace(fileName) ? "upload.bin" : fileName }
        };

        AddCloudinaryAuthFields(content, timestamp);

        using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cloudinary file upload failed with status {StatusCode}: {Payload}", response.StatusCode, payload);
            throw new InvalidOperationException($"Cloudinary rechazo el archivo: {payload}");
        }

        var parsed = JsonSerializer.Deserialize<CloudinaryUploadResponse>(payload);
        if (string.IsNullOrWhiteSpace(parsed?.SecureUrl))
        {
            throw new InvalidOperationException("Cloudinary no devolvio una URL segura para la imagen.");
        }

        return parsed.SecureUrl;
    }

    private string BuildSignature(string timestamp)
    {
        var parameters = new SortedDictionary<string, string>
        {
            ["timestamp"] = timestamp
        };

        if (!string.IsNullOrWhiteSpace(_uploadPreset))
        {
            parameters["upload_preset"] = _uploadPreset;
        }

        var toSign = string.Join("&", parameters.Select(parameter => $"{parameter.Key}={parameter.Value}"))
            + _credentials.ApiSecret;
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(toSign));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private void AddCloudinaryAuthFields(MultipartFormDataContent content, string timestamp)
    {
        if (!string.IsNullOrWhiteSpace(_uploadPreset))
        {
            _logger.LogInformation("Cloudinary upload is using signed CLOUDINARY_URL credentials with CLOUDINARY_UPLOAD_PRESET.");
            content.Add(new StringContent(_uploadPreset), "upload_preset");
        }
        else
        {
            _logger.LogInformation("Cloudinary upload is using signed CLOUDINARY_URL credentials.");
        }

        content.Add(new StringContent(_credentials.ApiKey), "api_key");
        content.Add(new StringContent(timestamp), "timestamp");
        content.Add(new StringContent(BuildSignature(timestamp)), "signature");
    }

    private static string? NormalizeOptionalEnvironmentValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().Trim('"', '\'');
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

            return new CloudinaryCredentials(
                uri.Host,
                Uri.UnescapeDataString(userInfo[0]),
                Uri.UnescapeDataString(userInfo[1]));
        }
    }
}
