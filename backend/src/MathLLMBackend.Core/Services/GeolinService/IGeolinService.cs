using MathLLMBackend.GeolinClient.Models;

namespace MathLLMBackend.Core.Services.GeolinService;

public interface IGeolinService
{
    Task<ProblemPageResponse> GetProblems(int page, int size, string? prefixName = "", CancellationToken ct = default);
} 