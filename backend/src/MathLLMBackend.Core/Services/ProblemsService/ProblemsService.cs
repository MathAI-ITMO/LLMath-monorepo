using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
using MathLLMBackend.ProblemsClient;
using MathLLMBackend.ProblemsClient.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace MathLLMBackend.Core.Services.ProblemsService;

public class ProblemsService : IProblemsService
{
    private readonly IProblemsAPI _problemsApi;
    private readonly IGeolinApi _geolinApi;
    private readonly ILogger<ProblemsService> _logger;
    public ProblemsService(IProblemsAPI problemsApi, IGeolinApi geolinApi, ILogger<ProblemsService> logger)
    {
        _problemsApi = problemsApi;
        _geolinApi = geolinApi;
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
            _logger.LogError(ex, "Error fetching problems from external problems service {message}", ex.Message);
            throw;
        }
    }
    public async Task<List<Problem>> SaveProblems(string name, string problemHash, int variationCount, CancellationToken ct = default)
    {
        try
        {
            List<Problem> result = new();
            for (int i = 0; i < variationCount; ++i)
            {
                int seed = new Random().Next();
                var problem = await _geolinApi.GetProblemCondition(
                    new ProblemConditionRequest()
                    {
                        Hash = problemHash,
                        Seed = seed,
                        Lang = "ru"
                    });
                var problemMongo = new ProblemRequest()
                {
                    Statement = problem.Condition,
                    GeolinAnsKey = new GeolinKey()
                    {
                        Hash = problemHash,
                        Seed = seed
                    }
                };
                var createdProblem = await _problemsApi.CreateProblem(problemMongo);
                result.Add(createdProblem);
                var tmp = await _problemsApi.GiveANameProblem(new ProblemWithNameRequest()
                {
                    Name = name,
                    ProblemId = createdProblem.Id
                }
                );
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching problems from external problems service {message}", ex.Message);
            throw;
        }
    }
    public async Task<List<Problem>> GetSavedProblemsByNames(string name, CancellationToken ct = default)
    {
        try
        {
            var problems = await _problemsApi.GetAllProblemsByName(name);
            return problems;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("404 (Not Found)"))
            {
                return new List<Problem>();
            }
            _logger.LogError(ex, "Error fetching problems from external problems service {message}", ex.Message);
            throw;
        }  
    }
    public async Task<List<string>> GetAllNames(CancellationToken ct = default)
    {
        try
        {
            return await _problemsApi.GetNames();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching problems from external problems service {message}", ex.Message);
            throw;
        }
    }
}
