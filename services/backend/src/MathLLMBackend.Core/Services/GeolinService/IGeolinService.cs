using MathLLMBackend.Domain.Models;

namespace MathLLMBackend.Core.Services.GeolinService;

public interface IGeolinService
{
    Task<ProblemData> GetProblem(string prefix, int? seed = null, CancellationToken ct = default);
    Task<AnswerCheckResult> CheckAnswer(string hash, string answerAttempt, int seed, string problemParams, CancellationToken ct = default);
}