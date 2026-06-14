using Microsoft.AspNetCore.Http;
using Shared.Kernel.Correlation;

namespace Shared.Kernel.Observability;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CorrelationContextAccessor accessor)
    {
        var correlationId = context.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        accessor.CorrelationId = correlationId;
        context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;
        context.TraceIdentifier = correlationId;

        await _next(context);
    }
}
