using MathLLMBackend.DomainServices.UserService;
using MathLLMBackend.DomainServices.ChatService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathLLMBackend.DomainServices;

public class DomainServicesRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddTransient<IUserService, UserService.UserService>();
        services.AddTransient<IChatService, ChatService.ChatService>();
        return services;
    }
}
