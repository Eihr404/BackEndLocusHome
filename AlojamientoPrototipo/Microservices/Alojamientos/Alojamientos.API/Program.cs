using Microsoft.EntityFrameworkCore;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.API.Extensions;
using Alojamientos.API.Middleware;
using Shared.Kernel.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AlojamientosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConexionAlojamientos"))
           .UseLowerCaseNamingConvention());

builder.Services.AddApplicationServices();
builder.Services.AddSharedObservability("Alojamientos.API");

builder.Services.AddControllers();
builder.Services.AddGrpc();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddCustomCors();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseRouting();

app.MapControllers();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGrpcService<Alojamientos.API.GrpcServices.CalendarioGrpcService>()
   .EnableGrpcWeb();

app.Run();
