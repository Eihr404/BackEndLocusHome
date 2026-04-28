namespace Microservicio.Clientes.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "BookingCorsPolicy";

    public static IServiceCollection AddBookingCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy
                    .AllowAnyOrigin()   // En producción: .WithOrigins("https://mifrontend.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        return services;
    }
}
