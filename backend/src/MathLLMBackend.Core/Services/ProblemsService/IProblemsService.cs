using MathLLMBackend.ProblemsClient.Models;

namespace MathLLMBackend.Core.Services.ProblemsService;

public interface IProblemsService
{
    Task<List<Problem>> GetSavedProblems(CancellationToken ct = default);
    Task<Problem> SaveProblem(ProblemRequest problem, CancellationToken ct = default);
    Task<List<Problem>> GetSavedProblemsByNames(string name, CancellationToken ct = default);
    Task<List<string>> GetAllNames(CancellationToken ct = default);
} 
