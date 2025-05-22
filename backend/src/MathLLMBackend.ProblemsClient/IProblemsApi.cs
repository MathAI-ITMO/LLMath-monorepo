using MathLLMBackend.ProblemsClient.Models;
using Refit;

namespace MathLLMBackend.ProblemsClient;

public interface IProblemsAPI
{
    [Get("/api/problems")]
    Task<List<Problem>> GetProblems();

    [Post("/api/problems")]
    Task<Problem> CreateProblem([Body] ProblemRequest request);

    [Post("/api/give_a_name")]
    Task<string> GiveANameProblem([Body] ProblemWithNameRequest request);

    [Get("/api/get_problems_by_name")]
    Task<List<Problem>> GetAllProblemsByName([AliasAs("problem_name")] string name);

    [Get("/api/names")]
    Task<List<string>> GetTypes();

    [Get("/api/problems/{id}")]
    Task<Problem> GetProblemById([AliasAs("id")] string problemId);

    [Get("/api/get_problems_by_type")]
    Task<List<Problem>> GetProblemsByType([AliasAs("problem_type")] string typeName);
} 