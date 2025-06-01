using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathLLMBackend.DataAccess;

public static class DataAccessRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
            
        services.AddScoped<WarmupService>();
        
        return services;
    }
} 