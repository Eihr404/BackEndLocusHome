using Microservicio.Clientes.Api.Extensions;
using Microservicio.Clientes.Api.Middleware;

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microservicio.Clientes.Api.Filters;
using Microservicio.Clientes.Api.GrpcServices;
using Microservicio.Clientes.Api.GraphQL;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios (capas 1-3 + infraestructura) ────────────────────────────────
builder.Services.AddBookingServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddBookingCors();
builder.Services.AddBookingApiVersioning();
builder.Services.AddBookingSwagger();

// Rate Limiting (Protección Brute Force)
builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("GlobalPolicy", opt => {
        opt.PermitLimit = 30; // Máximo 30 peticiones...
        opt.Window = TimeSpan.FromSeconds(60); // ...cada 60 segundos
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2; // Si se pasan, encolar hasta 2 antes de rechazar
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

// Build the App
var app = builder.Build();

// ── Pipeline de la aplicación ──────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();   // Primero: captura todos los errores
app.UseRateLimiter();                              // Segundo: Rate Limiting
app.UseBookingSwagger();                           // Swagger en la raíz (/)
app.UseHttpsRedirection();
app.UseCors(CorsExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("GlobalPolicy");

// Map gRPC Services
app.MapGrpcService<ClientesGrpcService>();
app.MapGrpcService<Microservicio.Clientes.Api.Grpc.InventarioGrpcServiceImpl>();

// Map GraphQL endpoint (por defecto en /graphql)
app.MapGraphQL();

app.Run();

public partial class Program { }
