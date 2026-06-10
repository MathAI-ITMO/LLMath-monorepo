using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Constants;

namespace MathLLMBackend.DataAccess.Services;

public class WarmupService
{
    private readonly AppDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<WarmupService> _logger;

    public WarmupService(
        AppDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        ILogger<WarmupService> logger)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _logger = logger;
    }

    public virtual async Task WarmupAsync()
    {
        try
        {
            _logger.LogInformation("Starting database warmup...");

            if (_dbContext.Database.IsRelational())
            {
                await _dbContext.Database.MigrateAsync();
            }

            await SeedRolesAsync();

            _logger.LogInformation("Database warmup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database warmup");
            throw;
        }
    }

    protected async Task SeedRolesAsync()
    {
        var roles = new[] { Role.Admin, Role.User };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                _logger.LogInformation("Seeding role: {Role}", role);
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}