using System.Net.Http.Headers;
using Shared.Kernel.Correlation;

namespace Shared.Kernel.Observability;

public sealed class HttpCorrelationDelegatingHandler : DelegatingHandler
{
    private readonly CorrelationContextAccessor _correlationAccessor;

    public HttpCorrelationDelegatingHandler(CorrelationContextAccessor correlationAccessor)
    {
        _correlationAccessor = correlationAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = _correlationAccessor.CorrelationId ?? Guid.NewGuid().ToString("N");
        if (!request.Headers.Contains(CorrelationConstants.HeaderName))
        {
            request.Headers.Add(CorrelationConstants.HeaderName, correlationId);
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return base.SendAsync(request, cancellationToken);
    }
}
