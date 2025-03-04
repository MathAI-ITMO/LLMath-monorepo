using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MathLLMBackend.Infrastructure;
using MathLLMBackend.DomainServices;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using MathLLMBackend.Presentation.Middlewares;
using NLog;
using NLog.Web;
using MathLLMBackend.Presentation.Jwt;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

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

    builder.Services.AddScoped<JwtTokenHelper>();

    InfrastractureRegistrar.Configure(builder.Services, configuration);
    DomainServicesRegistrar.Configure(builder.Services, configuration);


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

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<AuthenticationMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
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
