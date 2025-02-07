using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MathLLMBackend.Infrastructure;
using MathLLMBackend.DomainServices;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MathLLMBackend.Presentation;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddScoped<JwtTokenHelper>();

InfrastractureRgistrar.Configure(builder.Services, configuration);
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

        // Add the security requirement for all endpoints
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
app.UseAuthorization();
app.MapControllers();

app.Run();

