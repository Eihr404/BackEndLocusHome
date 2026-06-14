using GraphQLGateway;
using GraphQLGateway.Clients;
using GraphQLGateway.GraphQL;
using Polly;
using Polly.Extensions.Http;
using Shared.Kernel.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedObservability("GraphQLGateway");

static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));

static IAsyncPolicy<HttpResponseMessage> BuildCircuitBreakerPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(15));

builder.Services
    .AddHttpClient<AlojamientosClient>((sp, client) =>
    {
        var settings = sp.GetRequiredService<IConfiguration>()
            .GetSection(ServiceEndpointSettings.SectionName)
            .Get<ServiceEndpointSettings>() ?? new ServiceEndpointSettings();

        client.BaseAddress = new Uri(settings.AlojamientosBaseUrl);
    })
    .AddHttpMessageHandler<HttpCorrelationDelegatingHandler>()
    .AddPolicyHandler(BuildRetryPolicy())
    .AddPolicyHandler(BuildCircuitBreakerPolicy());

builder.Services
    .AddHttpClient<ReservasClient>((sp, client) =>
    {
        var settings = sp.GetRequiredService<IConfiguration>()
            .GetSection(ServiceEndpointSettings.SectionName)
            .Get<ServiceEndpointSettings>() ?? new ServiceEndpointSettings();

        client.BaseAddress = new Uri(settings.ReservasBaseUrl);
    })
    .AddHttpMessageHandler<HttpCorrelationDelegatingHandler>()
    .AddPolicyHandler(BuildRetryPolicy())
    .AddPolicyHandler(BuildCircuitBreakerPolicy());

builder.Services
    .AddHttpClient<FacturacionClient>((sp, client) =>
    {
        var settings = sp.GetRequiredService<IConfiguration>()
            .GetSection(ServiceEndpointSettings.SectionName)
            .Get<ServiceEndpointSettings>() ?? new ServiceEndpointSettings();

        client.BaseAddress = new Uri(settings.FacturacionBaseUrl);
    })
    .AddHttpMessageHandler<HttpCorrelationDelegatingHandler>()
    .AddPolicyHandler(BuildRetryPolicy())
    .AddPolicyHandler(BuildCircuitBreakerPolicy());

builder.Services
    .AddHttpClient<UsuariosClient>((sp, client) =>
    {
        var settings = sp.GetRequiredService<IConfiguration>()
            .GetSection(ServiceEndpointSettings.SectionName)
            .Get<ServiceEndpointSettings>() ?? new ServiceEndpointSettings();

        client.BaseAddress = new Uri(settings.UsuariosBaseUrl);
    })
    .AddHttpMessageHandler<HttpCorrelationDelegatingHandler>()
    .AddPolicyHandler(BuildRetryPolicy())
    .AddPolicyHandler(BuildCircuitBreakerPolicy());

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<MarketplaceQuery>()
    .AddTypeExtension<AlojamientoType>()
    .AddTypeExtension<HabitacionType>()
    .AddTypeExtension<ReservaType>()
    .AddTypeExtension<ReservaResumenType>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors();

app.MapGraphQL("/graphql");
app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();
