using MathLLMBackend.Core;
using MathLLMBackend.DataAccess.Contexts;
using Microsoft.OpenApi.Models;
using MathLLMBackend.Presentation.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddHttpLogging(o => { });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            policy =>
            {
                policy.AllowAnyOrigin()  // Allow requests from any origin
                      .AllowAnyMethod()  // Allow any HTTP method (GET, POST, PUT, etc.)
                      .AllowAnyHeader(); // Allow any headers
            });
    });

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var configuration = builder.Configuration;

    CoreServicesRegistrar.Configure(builder.Services, configuration);
    
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
    
    builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
        .AddEntityFrameworkStores<AppDbContext>();
    
    builder.Services.AddAuthorization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    
    
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
    app.UseCors("AllowAllOrigins");

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
