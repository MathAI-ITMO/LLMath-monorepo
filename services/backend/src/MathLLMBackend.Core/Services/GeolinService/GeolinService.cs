using MathLLMBackend.Core.Constants;
using MathLLMBackend.Domain.Models;
using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
using System.Text.Json;

namespace MathLLMBackend.Core.Services.GeolinService;

public class GeolinService : IGeolinService
{
    private readonly IGeolinApi _geolinApi;
    private readonly Random _random = new();

    public GeolinService(
        IGeolinApi geolinApi)
    {
        _geolinApi = geolinApi;
    }

    public async Task<ProblemData> GetProblem(string prefix, int? seed = null, CancellationToken ct = default)
    {
        var problem = await FindProblemByPrefix(prefix, ct);
        if (problem == null)
        {
            throw new InvalidOperationException($"No problem found for prefix '{prefix}'.");
        }

        seed = seed ?? _random.Next(1, 1000000000);
        var condition = await _geolinApi.GetProblemCondition(new ProblemConditionRequest
        {
            Hash = problem.Hash,
            Seed = seed,
            Lang = LocalizationConstants.RussianLanguageCode
        }, ct);
        
        if (condition == null)
        {
            throw new InvalidOperationException("Failed to get problem condition from GeoLin.");
        }

        return new ProblemData
        {
            Name = problem.Name,
            Hash = problem.Hash,
            Condition = condition.Condition,
            Seed = seed!.Value,
            ProblemParams = condition.ProblemParams,
            Description = problem.Description
        };
    }

    public async Task<AnswerCheckResult> CheckAnswer(string hash, string answerAttempt, int seed, string problemParams, CancellationToken ct = default)
    {
        var verdict = await GetAnswerVerdict(hash, answerAttempt, seed, problemParams, ct);
        var isCorrect = verdict >= GeolinConstants.CorrectAnswerVerdictThreshold;

        return new AnswerCheckResult
        {
            IsCorrect = isCorrect,
            Verdict = verdict
        };
    }

    private async Task<ProblemInfoResponse> FindProblemByPrefix(string prefix, CancellationToken ct)
    {
        var problemPage = await _geolinApi.GetProblemsInfo(page: 1, size: 1, prefixName: prefix, ct: ct);
        
        if (problemPage.Problems.Count == 0)
        {
            throw new InvalidOperationException($"Problem with prefix '{prefix}' not found");
        }

        var problem = problemPage.Problems.First();

        return problem;
    }

    private async Task<double> GetAnswerVerdict(string hash, string answerAttempt, int seed, string problemParams, CancellationToken ct)
    {
        var checkRequest = new ProblemAnswerCheckRequest
        {
            Hash = hash,
            AnswerAttempt = answerAttempt,
            Seed = seed,
            ProblemParams = problemParams
        };

        var response = await _geolinApi.CheckProblemAnswer(checkRequest, ct);
        return response.Verdict;
    }
} 