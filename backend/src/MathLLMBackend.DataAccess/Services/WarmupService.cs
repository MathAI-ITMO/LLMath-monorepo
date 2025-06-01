using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MathLLMBackend.DataAccess.Contexts;

namespace MathLLMBackend.DataAccess.Services;

public class WarmupService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<WarmupService> _logger;

    public WarmupService(AppDbContext dbContext, ILogger<WarmupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task WarmupAsync()
    {
        try
        {
            _logger.LogInformation("Starting database warmup...");
            
            await _dbContext.Database.MigrateAsync();
            
            _logger.LogInformation("Database warmup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database warmup");
            throw;
        }
    }
} 