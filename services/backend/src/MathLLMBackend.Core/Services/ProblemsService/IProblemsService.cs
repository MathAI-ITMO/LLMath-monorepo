using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Models;

namespace MathLLMBackend.Core.Services.ProblemsService;

public interface IProblemsService
{
    Task<IEnumerable<Problem>> GetProblems(CancellationToken ct);
    Task<IEnumerable<Problem>> GetProblemsByType(TaskType taskType, CancellationToken ct);
    Task<Problem?> GetProblem(Guid problemId, CancellationToken ct);
    Task<Problem?> UpdateProblem(Guid problemId, ProblemUpdateModel model, CancellationToken ct);
    Task<Problem> CreateProblem(ProblemUpdateModel model, CancellationToken ct);
    Task DeleteProblem(Guid problemId, CancellationToken ct);
}
