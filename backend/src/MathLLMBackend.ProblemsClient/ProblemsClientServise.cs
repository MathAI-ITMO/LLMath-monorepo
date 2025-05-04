using MathLLMBackend.ProblemsClient.Options;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace MathLLMBackend.ProblemsClient;

public static class ProblemsClientRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, Action<ProblemsClientOptions> configuration)
    {
        var config = new ProblemsClientOptions();
        configuration(config);
        services.Configure(configuration)
        .AddRefitClient<IProblemsAPI>()
        .ConfigureHttpClient(c => 
        {
            c.BaseAddress = new Uri(config.BaseAddress);
        });
        
        return services;        
    }
}