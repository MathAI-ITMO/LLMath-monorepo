using MathLLMBackend.DataAccess.Configuration;
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

        services.AddScoped<WarmupService>();

        return services;
    }
}
