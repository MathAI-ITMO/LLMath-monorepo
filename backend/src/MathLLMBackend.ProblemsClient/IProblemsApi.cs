using MathLLMBackend.ProblemsClient.Models;
using Refit;

namespace MathLLMBackend.ProblemsClient;

public interface IProblemsAPI
{
    [Get("/api/problems")]
    Task<List<Problem>> GetProblems();

    [Post("/api/problems")]
    Task<Problem> CreateProblem([Body] ProblemRequest request);
} 