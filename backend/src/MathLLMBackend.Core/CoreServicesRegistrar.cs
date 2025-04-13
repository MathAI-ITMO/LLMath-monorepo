using MathLLMBackend.Core.Configuration;
using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Core.Services.GeolinService;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.Core.Services.PromptService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathLLMBackend.Core;

public class CoreServicesRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddTransient<IChatService, ChatService>();
        services.AddTransient<ILlmService, LlmService>();
        services.AddTransient<IPromptService, PromptService>();
        services.AddTransient<IGeolinService, GeolinService>();
        
        services.Configure<LlmServiceConfiguration>(configuration.GetSection("OpenAi"));
        services.Configure<PromptConfiguration>(configuration.GetSection("DefaultPrompts"));
        return services;
    }
}
