using MathLLMBackend.Core;
using MathLLMBackend.DataAccess;
using MathLLMBackend.DataAccess.Services;
using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Options;
using Microsoft.OpenApi.Models;
using MathLLMBackend.Presentation.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MathLLMBackend.DataAccess.Contexts;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddHttpLogging(o => { });
    var configuration = builder.Configuration;

    builder.Services.AddCors(options =>
    {
        var origins = configuration["CorsOrigins"] ?? throw new Exception("CorsOrigins not found");
        options.AddDefaultPolicy(
            policy =>
            {
                policy.WithOrigins("http://localhost:23188")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
    });

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    CoreServicesRegistrar.Configure(builder.Services, configuration);
    GeolinClientRegistrar.Configure(builder.Services, configuration.GetSection(nameof(GeolinClientOptions)).Bind);
    DataAccessRegistrar.Configure(builder.Services, configuration);
    
    builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
        .AddEntityFrameworkStores<AppDbContext>();
    
    builder.Services.AddAuthorization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = false;
        // Other cookie options if needed
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.Cookie.SameSite = SameSiteMode.Lax; // Or Strict depending on your needs
    });
    
    
    
    builder.Services.AddSwaggerGen(c =>
        {
            var openApiSecurityScheme = new OpenApiSecurityScheme()
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer [space] {your token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            };

            c.AddSecurityDefinition("Bearer", openApiSecurityScheme);

            var openApiSecurityRequirement = new OpenApiSecurityRequirement()
            {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
            };

            c.AddSecurityRequirement(openApiSecurityRequirement);
        });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var warmupService = scope.ServiceProvider.GetRequiredService<WarmupService>();
        await warmupService.WarmupAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    app.MapIdentityApi<IdentityUser>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseHttpLogging();
    app.UseCors();

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
