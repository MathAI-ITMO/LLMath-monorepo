using MathLLMBackend.DomainServices.UserService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathLLMBackend.DomainServices;

public class DomainServicesRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddTransient<IUserService, UserService.UserService>();
        return services;
    }
}
