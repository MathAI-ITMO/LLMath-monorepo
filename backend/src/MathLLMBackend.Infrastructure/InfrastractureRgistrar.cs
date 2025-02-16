using MathLLMBackend.Infrastructure.Repositories;
using MathLLMBackend.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathLLMBackend.Infrastructure;

public static class InfrastractureRgistrar
{
    public static IServiceCollection Configure(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddSingleton<IUsersRepository, UserRepository>();
        services.AddSingleton<IIdentityRepository, IdentityRepository>();
        services.AddSingleton<IChatRepository, ChatRepository>();
        services.AddSingleton<IMsgRepository, MsgRepository>();

        var connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' not found in appsettings.json");
        services.AddSingleton(_ => new DataContext(connectionString));
        return services;
    }

}
