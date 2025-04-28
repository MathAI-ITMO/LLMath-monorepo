using MathLLMBackend.ProblemsClient;
using MathLLMBackend.ProblemsClient.Models;
using Microsoft.Extensions.Logging;

namespace MathLLMBackend.Core.Services.ProblemsService;

public class ProblemsService : IProblemsService
{
    private readonly IProblemsAPI _problemsApi;
    private readonly ILogger<ProblemsService> _logger;
    public ProblemsService(IProblemsAPI problemsApi, ILogger<ProblemsService> logger)
    {
        _problemsApi = problemsApi;
        _logger = logger;
    }
    public async Task<List<Problem>> GetSavedProblems(CancellationToken ct = default)
    {
        try  
        {
            return await _problemsApi.GetProblems();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching problems from mongodb");
            throw;
        }
    }
    public async Task<Problem> SaveProblem(ProblemRequest problem, CancellationToken ct = default)
    {
        try
        {
            return await _problemsApi.CreateProblem(problem);
        }
                catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving problem in mongodb");
            throw;
        }
    }
}
