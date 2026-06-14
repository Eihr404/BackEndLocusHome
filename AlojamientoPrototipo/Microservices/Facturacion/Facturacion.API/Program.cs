using Microsoft.EntityFrameworkCore;
using Facturacion.DataAccess.Contexts;
using Facturacion.API.Extensions;
using Facturacion.API.Middleware;
using MassTransit;
using Shared.Kernel.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FacturacionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConexionFacturacion"))
           .UseLowerCaseNamingConvention());

builder.Services.AddApplicationServices();
builder.Services.AddSharedObservability("Facturacion.API");

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rmqUrl = builder.Configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rmqUrl))
        {
            cfg.Host(new Uri(rmqUrl));
        }
        else
        {
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }

        cfg.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(2)));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddCustomCors();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();

app.Run();
