using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
using Microsoft.Extensions.Logging;

namespace MathLLMBackend.Core.Services.GeolinService;

public class GeolinService : IGeolinService
{
    private readonly IGeolinApi _geolinApi;
    private readonly ILogger<GeolinService> _logger;

    public GeolinService(IGeolinApi geolinApi, ILogger<GeolinService> logger)
    {
        _geolinApi = geolinApi;
        _logger = logger;
    }

    public async Task<ProblemPageResponse> GetProblems(int page, int size, string? prefixName = "", CancellationToken ct = default)
    {
        try
        {
            return await _geolinApi.GetProblemsInfo(page, size, prefixName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching problems from geolin");
            throw;
        }
    }
} 