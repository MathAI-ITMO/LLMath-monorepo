using MathLLMBackend.Core.Configuration;
using MathLLMBackend.Core.Services.AuthService;
using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Core.Services.GeolinService;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.Core.Services.PromptService;
using MathLLMBackend.Core.Services.ProblemsService;
using MathLLMBackend.Core.Services;
using MathLLMBackend.Core.Services.StatsService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathLLMBackend.Core;

public static class CoreServicesRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IChatService, ChatService>();
        services.AddTransient<ILlmService, LlmService>();
        services.AddTransient<IPromptService, PromptService>();
        services.AddTransient<IProblemsService, ProblemsService>();
        services.AddTransient<IUserTaskService, UserTaskService>();
        services.AddTransient<IStatsService, StatsService>();
        
        services.Configure<LlmServiceConfiguration>(configuration.GetSection("OpenAi"));
        services.Configure<PromptConfiguration>(configuration.GetSection("DefaultPrompts"));
        
        return services;
    }
}
