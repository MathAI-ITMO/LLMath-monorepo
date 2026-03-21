using MathLLMBackend.GeolinClient.Models;
using Refit;

namespace MathLLMBackend.GeolinClient;

public interface IGeolinApi
{
    [Post("/problems-info-by-hashes")]
    [Headers("Authorization: Basic")]
    Task<List<ProblemInfoResponse>> GetProblemsInfoByHashes([Body] ProblemInfoRequest request, CancellationToken ct = default);

    [Get("/problems-info/{page}/{size}")]
    [Headers("Authorization: Basic")]
    Task<ProblemPageResponse> GetProblemsInfo(
        [AliasAs("page")] int page,
        [AliasAs("size")] int size,
        [AliasAs("prefixName")] string? prefixName = "",
        CancellationToken ct = default);

    [Post("/problem-condition")]
    [Headers("Authorization: Basic")]
    Task<ProblemConditionResponse> GetProblemCondition([Body] ProblemConditionRequest request, CancellationToken ct = default);

    [Post("/problems-conditions")]
    [Headers("Authorization: Basic")]
    Task<ProblemsConditionsResponse> GetProblemsConditions([Body] ProblemsConditionsRequest request, CancellationToken ct = default);

    [Post("/problem-answer-check")]
    [Headers("Authorization: Basic")]
    Task<ProblemAnswerCheckResponse> CheckProblemAnswer([Body] ProblemAnswerCheckRequest request, CancellationToken ct = default);
} 