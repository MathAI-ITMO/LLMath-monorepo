using MathLLMBackend.Core.Contexts;
using MathLLMBackend.DataAccess.Configuration;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.DataAccess.Services;
using MathLLMBackend.DataAccess.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathLLMBackend.DataAccess;

public static class DataAccessRegistrar
{
    public static IServiceCollection Configure(IServiceCollection services, IConfiguration configuration)
    {
        var dbConfig = configuration.GetSection(DatabaseConfiguration.SectionName).Get<DatabaseConfiguration>()
            ?? new DatabaseConfiguration { Provider = DatabaseProvider.Postgres };

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (dbConfig.Provider)
            {
                case DatabaseProvider.InMemory:
                    var dbName = dbConfig.InMemoryDatabaseName ?? "InMemoryDb";
                    options.UseInMemoryDatabase(dbName);
                    break;
                case DatabaseProvider.Postgres:
                default:
                    var connectionString = configuration.GetConnectionString("Postgres")
                        ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
                    options.UseNpgsql(connectionString);
                    break;
            }
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<WarmupService>();

        return services;
    }

    public static IdentityBuilder ConfigureIdentity(IdentityBuilder builder)
    {
        return builder
            .AddEntityFrameworkStores<AppDbContext>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();
    }

    public static async Task WarmupAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var warmupService = scope.ServiceProvider.GetRequiredService<WarmupService>();
        await warmupService.WarmupAsync();
    }
}
