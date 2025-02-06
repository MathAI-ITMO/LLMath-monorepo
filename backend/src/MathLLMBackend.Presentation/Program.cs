using Microsoft.AspNetCore.Authentication;
using MathLLMBackend.Infrastructure;
using MathLLMBackend.DomainServices;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

InfrastractureRgistrar.Configure(builder.Services, configuration);
DomainServicesRegistrar.Configure(builder.Services, configuration);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


app.Run();

