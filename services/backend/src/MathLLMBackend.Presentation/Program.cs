using MathLLMBackend.Core;
using MathLLMBackend.DataAccess;
using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Options;
using Scalar.AspNetCore;
using MathLLMBackend.Presentation.Middlewares;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Identity;
using MathLLMBackend.Presentation.Configuration;
using MathLLMBackend.Domain.Entities;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddHttpLogging(o => { });
    var configuration = builder.Configuration;
    var corsConfiguration = configuration.GetSection(nameof(CorsConfiguration)).Get<CorsConfiguration>() ?? new CorsConfiguration();

    if (corsConfiguration.Enabled)
    {
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins(corsConfiguration.Origin.Split(';'))
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
        });
    }

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    CoreServicesRegistrar.Configure(builder.Services, configuration);
    GeolinClientRegistrar.Configure(builder.Services, configuration.GetSection(nameof(GeolinClientOptions)).Bind);
    DataAccessRegistrar.Configure(builder.Services, configuration);

    var identityBuilder = builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
        .AddRoles<IdentityRole>();
    DataAccessRegistrar.ConfigureIdentity(identityBuilder);

    builder.Services.AddAuthorization();

    builder.Services.AddControllers(options =>
    {
        const int firstBinderIndex = 0;
        options.ModelBinderProviders.Insert(firstBinderIndex, new MathLLMBackend.Presentation.Binders.UserIdModelBinderProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

    builder.Services.AddOpenApi("openapi", options =>
    {
        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer [space] {your token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                }
            };
            return Task.CompletedTask;
        });
        options.AddOperationTransformer<MathLLMBackend.Presentation.Configuration.FromUserIdOperationTransformer>();
        options.AddSchemaTransformer<MathLLMBackend.Presentation.Configuration.EnumSchemaTransformer>();
        options.AddOperationTransformer((operation, context, ct) =>
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", null),
                        new List<string>()
                    }
                }
            ];
            return Task.CompletedTask;
        });
    });

    var app = builder.Build();

    var isOpenApiGeneration = Environment.GetEnvironmentVariable("OPENAPI_GENERATION") == "true";
    if (!isOpenApiGeneration)
    {
        await DataAccessRegistrar.WarmupAsync(app.Services);
    }

    app.UseHttpLogging();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    if (corsConfiguration.Enabled)
    {
        app.UseCors();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapIdentityApi<ApplicationUser>();
    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
