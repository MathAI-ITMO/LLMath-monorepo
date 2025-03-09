using MathLLMBackend.Core.Configuration;
using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Core.Services.LlmService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathLLMBackend.Core;

public class CoreServicesRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddTransient<IChatService, ChatService>();
        services.AddTransient<ILlmService, LlmService>();
        
        services.Configure<LlmServiceConfiguration>(configuration.GetSection("OpenAi"));
        return services;
    }
}
