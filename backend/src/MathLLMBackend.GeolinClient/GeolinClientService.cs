using MathLLMBackend.GeolinClient.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace MathLLMBackend.GeolinClient;

public static class GeolinClientRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, ConfigurationManager configuration)
    {
        var config = configuration.GetSection("Geolin").Get<GeolinClientOptions>() 
                     ?? throw new ApplicationException("Geolin client configuration is missing");
        
        services.AddRefitClient<IGeolinApi>()
            .ConfigureHttpClient(c =>
            {
                if (config.AuthorizationHeader is not null)
                {
                    c.DefaultRequestHeaders.Add("Authoirzation", config.AuthorizationHeader);
                }

                c.BaseAddress = new Uri(config.BaseAddress);
            });

        return services;
    }
}