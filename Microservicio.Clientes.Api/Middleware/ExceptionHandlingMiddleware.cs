using Microservicio.Clientes.Api.Models.Common;
using Microservicio.Clientes.Business.Exceptions;
using System.Net;
using System.Text.Json;

namespace Microservicio.Clientes.Api.Middleware;

/// <summary>
/// Intercepta todas las excepciones no controladas y las convierte en respuestas JSON
/// con el código HTTP correcto. Sin este middleware, Swagger devolvería HTML en errores.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, mensaje, errores) = ex switch
        {
            NotFoundException   nfe => (HttpStatusCode.NotFound,             nfe.Message, new List<string>()),
            ValidationException ve  => (HttpStatusCode.UnprocessableEntity,  ve.Message,  ve.Errors.ToList()),
            ConflictException   ce  => (HttpStatusCode.Conflict,             ce.Message,  new List<string>()),
            BusinessException   be  => (HttpStatusCode.BadRequest,           be.Message,  new List<string>()),
            _                       => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado en el servidor.", new List<string>())
        };

        var response = new ApiErrorResponse
        {
            Exitoso    = false,
            Mensaje    = mensaje,
            Errores    = errores,
            StatusCode = (int)statusCode,
            TraceId    = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
