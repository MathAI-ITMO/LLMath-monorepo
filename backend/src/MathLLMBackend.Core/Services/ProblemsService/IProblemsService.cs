using MathLLMBackend.ProblemsClient.Models;

namespace MathLLMBackend.Core.Services.ProblemsService;

public interface IProblemsService
{
    Task<List<Problem>> GetSavedProblems(CancellationToken ct = default);
    Task<Problem> SaveProblem(ProblemRequest problem, CancellationToken ct = default);
} 
