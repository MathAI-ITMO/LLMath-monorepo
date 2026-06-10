using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.DataAccess.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathLLMBackend.IntegrationTests;

internal class TestWarmupService : WarmupService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<WarmupService> _logger;

    public TestWarmupService(
        AppDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        ILogger<WarmupService> logger)
        : base(dbContext, roleManager, logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override async Task WarmupAsync()
    {
        _logger.LogInformation("Starting database warmup...");

        var isInMemory = _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        if (isInMemory)
        {
            await _dbContext.Database.EnsureCreatedAsync();
            await SeedRolesAsync();
            _logger.LogInformation("Database warmup completed successfully (InMemory)");
        }
        else
        {
            await base.WarmupAsync();
        }
    }
}
