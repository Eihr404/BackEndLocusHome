using System.Net;
using System.Text.Json;
using HotChocolate;

namespace GraphQLGateway.Clients;

internal static class GraphQlHttpErrorHelper
{
    public static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string serviceName,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var (message, details) = TryExtractError(body);

        throw BuildException(
            response.StatusCode,
            serviceName,
            operation,
            message,
            details,
            body);
    }

    private static Exception BuildException(
        HttpStatusCode statusCode,
        string serviceName,
        string operation,
        string? message,
        string? details,
        string rawBody)
    {
        var fallbackMessage = $"{operation} fallo en {serviceName}.";
        var errorMessage = string.IsNullOrWhiteSpace(message) ? fallbackMessage : message;

        var error = ErrorBuilder.New()
            .SetMessage(errorMessage)
            .SetExtension("service", serviceName)
            .SetExtension("httpStatus", (int)statusCode);

        if (!string.IsNullOrWhiteSpace(details))
        {
            error.SetExtension("details", details);
        }

        switch (statusCode)
        {
            case HttpStatusCode.BadRequest:
                error.SetCode("BAD_REQUEST");
                break;
            case HttpStatusCode.Unauthorized:
                error.SetCode("AUTH_INVALID_CREDENTIALS");
                break;
            case HttpStatusCode.NotFound:
                error.SetCode("RESOURCE_NOT_FOUND");
                break;
            case HttpStatusCode.Conflict:
                error.SetCode("CONFLICT");
                break;
            default:
                error.SetCode((int)statusCode >= 500 ? "UPSTREAM_SERVICE_ERROR" : "UPSTREAM_HTTP_ERROR");
                if ((int)statusCode >= 500 && string.IsNullOrWhiteSpace(details))
                {
                    error.SetExtension("details", "Ocurrio un error interno en el servicio aguas arriba.");
                }
                break;
        }

        if (!string.IsNullOrWhiteSpace(rawBody) && string.IsNullOrWhiteSpace(message))
        {
            error.SetExtension("rawBody", rawBody);
        }

        return new GraphQLException(error.Build());
    }

    private static (string? Message, string? Details) TryExtractError(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return (null, null);
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            string? Read(params string[] names)
            {
                foreach (var name in names)
                {
                    if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        return value.GetString();
                    }
                }

                return null;
            }

            return (
                Read("message", "Message", "mensaje", "Mensaje"),
                Read("details", "Details", "diagnostico", "Diagnostico"));
        }
        catch (JsonException)
        {
            return (body, null);
        }
    }
}
