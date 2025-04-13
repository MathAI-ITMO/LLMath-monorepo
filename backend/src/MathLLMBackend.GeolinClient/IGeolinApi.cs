using MathLLMBackend.GeolinClient.Models;
using Refit;

namespace MathLLMBackend.GeolinClient;

public interface IGeolinApi
{
    [Post("/problems-info-by-hashes")]
    Task<List<ProblemInfoResponse>> GetProblemsInfoByHashes([Body] ProblemInfoRequest request);

    [Get("/problems-info/{page}/{size}")]
    Task<ProblemPageResponse> GetProblemsInfo(
        [AliasAs("page")] int page,
        [AliasAs("size")] int size,
        [AliasAs("prefixName")] string? prefixName = "");

    [Post("/problem-condition")]
    Task<ProblemConditionResponse> GetProblemCondition([Body] ProblemConditionRequest request);

    [Post("/problems-conditions")]
    Task<ProblemsConditionsResponse> GetProblemsConditions([Body] ProblemsConditionsRequest request);

    [Post("/problem-answer-check")]
    Task<ProblemAnswerCheckResponse> CheckProblemAnswer([Body] ProblemAnswerCheckRequest request);
} 