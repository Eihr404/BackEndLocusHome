using Microservicio.Clientes.Api.Extensions;
using Microservicio.Clientes.Api.Middleware;

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microservicio.Clientes.Api.Filters;
using Microservicio.Clientes.Api.GrpcServices;
using Microservicio.Clientes.Api.GraphQL;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Forzar puerto para despliegue (0.0.0.0 es necesario para Docker/Render)
builder.WebHost.UseUrls("http://0.0.0.0:5028");

// ── Servicios (capas 1-3 + infraestructura) ────────────────────────────────
builder.Services.AddBookingServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddBookingCors();
builder.Services.AddBookingApiVersioning();
builder.Services.AddBookingSwagger();

// Rate Limiting (Protección Brute Force)
builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("GlobalPolicy", opt => {
        opt.PermitLimit = 300; // Máximo 300 peticiones...
        opt.Window = TimeSpan.FromSeconds(60); // ...cada 60 segundos
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 50; // Si se pasan, encolar hasta 50 antes de rechazar
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Registrar Filtro AntiXSS global
builder.Services.AddControllers(options => {
    options.Filters.Add<AntiXssFilterAttribute>();
});
builder.Services.AddMemoryCache(); // Necesario para Idempotency en Pagos

// gRPC (Semana 3)
builder.Services.AddGrpc();

// GraphQL (Semana 4)
builder.Services.AddGraphQLServer()
                .AddQueryType<Query>();

// Mensajería y EDA (Semana 5 y 6)
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// gRPC client removed — availability check now handled in-process via IPropiedadService

// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Build the App
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();
app.UseCors("AllowAll");

app.UseBookingSwagger();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("GlobalPolicy");

// En la API pura no necesitamos FallbackToFile porque el Front-end vive en su propio contenedor
app.Run();

public partial class Program { }
