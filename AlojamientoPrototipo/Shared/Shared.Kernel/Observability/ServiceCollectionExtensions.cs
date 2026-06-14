using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shared.Kernel.Observability;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedObservability(this IServiceCollection services, string serviceName)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<Shared.Kernel.Correlation.CorrelationContextAccessor>();
        services.AddTransient<HttpCorrelationDelegatingHandler>();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(serviceName)
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddConsoleExporter());

        return services;
    }
}
