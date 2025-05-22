using MathLLMBackend.ProblemsClient.Models;

namespace MathLLMBackend.Core.Services.ProblemsService;

public interface IProblemsService
{
    Task<List<Problem>> GetSavedProblems(CancellationToken ct = default);
    Task<List<Problem>> SaveProblems(string name, string problemHash, int variationCount, CancellationToken ct = default);
    Task<List<Problem>> GetSavedProblemsByNames(string name, CancellationToken ct = default);
    Task<List<string>> GetAllTypes(CancellationToken ct = default);
    Task<Problem?> GetProblemFromDbAsync(string problemDbId, CancellationToken ct = default);
    Task<List<Problem>> GetSavedProblemsByTypes(string typeName, CancellationToken ct = default);
} 
