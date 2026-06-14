using MassTransit;
using Polly;
using Polly.Extensions.Http;
using Shared.Kernel.Observability;
using SoapFacturaGateway.Consumers;
using SoapFacturaGateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedObservability("SoapFacturaGateway");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SoapEnvelopeBuilder>();
builder.Services.AddSingleton<FacturaXmlBuilder>();
builder.Services.AddScoped<SoapFacturaDispatchService>();

builder.Services.AddHttpClient<FacturacionApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["ServiceEndpoints:FacturacionBaseUrl"] ?? "https://israel-facturacion-api.onrender.com");
})
.AddHttpMessageHandler<HttpCorrelationDelegatingHandler>()
.AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(250 * attempt)));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PagoAprobadoConsumer>();
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

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
