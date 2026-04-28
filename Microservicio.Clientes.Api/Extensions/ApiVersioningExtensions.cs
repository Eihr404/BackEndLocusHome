using Asp.Versioning;

namespace Microservicio.Clientes.Api.Extensions;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddBookingApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion                = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions               = true;  // headers: api-supported-versions
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat           = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
