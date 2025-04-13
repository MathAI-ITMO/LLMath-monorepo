using MathLLMBackend.GeolinClient.HttpMessageHandlers;
using MathLLMBackend.GeolinClient.Options;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace MathLLMBackend.GeolinClient;

public static class GeolinClientRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, Action<GeolinClientOptions> configuration)
    {
        var config = new GeolinClientOptions();
        configuration(config);

        services
            .Configure(configuration)
            .AddTransient<AuthMessageHandler>()
            .AddRefitClient<IGeolinApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(config.BaseAddress);
            })
            .AddHttpMessageHandler<AuthMessageHandler>();

        return services;
    }
}