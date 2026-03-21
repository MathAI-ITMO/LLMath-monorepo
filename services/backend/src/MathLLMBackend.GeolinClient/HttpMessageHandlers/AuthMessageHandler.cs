using System.Net.Http.Headers;
using MathLLMBackend.GeolinClient.Options;
using Microsoft.Extensions.Options;

namespace MathLLMBackend.GeolinClient.HttpMessageHandlers;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<GeolinClientOptions> _options;
    
    public AuthMessageHandler(IOptionsMonitor<GeolinClientOptions> options)
    {
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var header = _options.CurrentValue.AuthorizationHeader?.Split();
        if (header is not null && header.Length == 2)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(header[0], header[1]);
        } 
        
        return await base.SendAsync(request, cancellationToken);
    }
}