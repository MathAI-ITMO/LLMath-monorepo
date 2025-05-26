using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MathLLMBackend.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/geolin-proxy")]
    public class GeolinProxyController : ControllerBase
    {
        private readonly IGeolinApi _geolinApi;
        private readonly ILogger<GeolinProxyController> _logger;
        private readonly Random _random;

        public GeolinProxyController(IGeolinApi geolinApi, ILogger<GeolinProxyController> logger)
        {
            _geolinApi = geolinApi ?? throw new ArgumentNullException(nameof(geolinApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _random = new Random();
        }

        public class GeolinProblemDataResponse
        {
            public string? Name { get; set; }
            public string? Hash { get; set; }
            public string? Condition { get; set; }
            public int? Seed { get; set; }
            public string? Error { get; set; }
            
            [JsonPropertyName("problemParams")]
            public string? ProblemParams { get; set; }
        }

        [HttpGet("problem-data")]
        public async Task<IActionResult> GetProblemDataByPrefix(
            [FromQuery] string prefix, 
            [FromQuery] int? seed = null)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return BadRequest(new GeolinProblemDataResponse { Error = "Prefix cannot be empty." });
            }

            try
            {
                _logger.LogInformation("Attempting to get problems info for prefix: {Prefix}", prefix);
                
                // 1. Получаем информацию о задаче по префиксу (имени)
                ProblemPageResponse problemPage = await _geolinApi.GetProblemsInfo(page: 1, size: 10, prefixName: prefix);

                if (problemPage == null || problemPage.Problems == null || !problemPage.Problems.Any())
                {
                    _logger.LogWarning("No problems found for prefix: {Prefix}", prefix);
                    return NotFound(new GeolinProblemDataResponse { Error = $"No problem found for prefix '{prefix}'." });
                }

                // Пытаемся найти точное совпадение по имени
                ProblemInfoResponse problemInfo = null;
                if (problemPage.Problems.Count == 1)
                {
                    // Если вернулась только одна задача, используем ее
                    problemInfo = problemPage.Problems.First();
                } 
                else 
                {
                    // Если вернулось несколько задач, ищем точное совпадение по имени
                    problemInfo = problemPage.Problems.FirstOrDefault(p => p.Name == prefix);
                    
                    // Если точного совпадения нет, берем первую задачу
                    if (problemInfo == null)
                    {
                        _logger.LogInformation("No exact match found for prefix {Prefix}. Using first problem from list.", prefix);
                        problemInfo = problemPage.Problems.First();
                    }
                }
                
                _logger.LogInformation("Found problem info: Name='{ProblemName}', Hash='{ProblemHash}' for prefix: {Prefix}", 
                    problemInfo.Name, problemInfo.Hash, prefix);

                if (string.IsNullOrWhiteSpace(problemInfo.Hash))
                {
                    _logger.LogWarning("Problem info for prefix '{Prefix}' has an empty hash.", prefix);
                    return StatusCode(500, new GeolinProblemDataResponse { Error = "Problem hash received from GeoLin is empty." });
                }

                // Определяем seed для запроса
                int seedToUse;
                
                if (seed.HasValue)
                {
                    // Используем seed из параметров запроса, если он указан
                    seedToUse = seed.Value;
                    _logger.LogInformation("Using provided seed: {Seed} from request parameters", seedToUse);
                }
                else
                {
                    // Генерируем случайный seed в диапазоне от 1 до 1 000 000 000
                    seedToUse = _random.Next(1, 1000000000);
                    _logger.LogInformation("Generated random seed: {Seed}", seedToUse);
                }
                
                // 2. Получаем условие задачи с указанным seed
                ProblemConditionRequest conditionRequest = new ProblemConditionRequest
                {
                    Hash = problemInfo.Hash,
                    Seed = seedToUse,
                    Lang = "ru"
                };

                _logger.LogInformation("Requesting problem condition for hash: {Hash} with Seed={Seed}", 
                    problemInfo.Hash, seedToUse);
                
                ProblemConditionResponse conditionResponse = await _geolinApi.GetProblemCondition(conditionRequest);

                if (conditionResponse == null)
                {
                    _logger.LogWarning("No condition response for hash: {Hash}", problemInfo.Hash);
                    return StatusCode(500, new GeolinProblemDataResponse { Error = "Failed to get problem condition from GeoLin."});
                }
                
                _logger.LogInformation("Received condition from GeoLin: '{ProblemCondition}'", 
                    conditionResponse.Condition?.Substring(0, Math.Min(50, conditionResponse.Condition?.Length ?? 0)) + "...");
                _logger.LogInformation("Received problem_params from GeoLin: '{ProblemParams}'", conditionResponse.ProblemParams);

                // В большинстве случаев мы можем использовать наш seed
                int? extractedSeed = seedToUse;
                
                // Но для надежности проверим, не указан ли seed в параметрах ответа
                if (!string.IsNullOrWhiteSpace(conditionResponse.ProblemParams))
                {
                    try
                    {
                        using JsonDocument document = JsonDocument.Parse(conditionResponse.ProblemParams);
                        if (document.RootElement.TryGetProperty("seed", out JsonElement seedElement) && 
                            seedElement.ValueKind == JsonValueKind.Number)
                        {
                            int seedFromParams = seedElement.GetInt32();
                            if (seedFromParams != seedToUse)
                            {
                                _logger.LogWarning("Seed in problem_params {SeedFromParams} differs from requested seed {RequestedSeed}. Using the one from problem_params.", 
                                    seedFromParams, seedToUse);
                                extractedSeed = seedFromParams;
                            }
                            else
                            {
                                _logger.LogInformation("Confirmed that our seed {Seed} matches the one in problem_params", extractedSeed);
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Failed to parse ProblemParams as JSON: {ProblemParams}, using our seed {Seed}", 
                            conditionResponse.ProblemParams, seedToUse);
                    }
                }

                var response = new GeolinProblemDataResponse
                {
                    Name = problemInfo.Name,
                    Hash = problemInfo.Hash,
                    Condition = conditionResponse.Condition,
                    Seed = extractedSeed,
                    ProblemParams = conditionResponse.ProblemParams
                };
                
                _logger.LogInformation("Successfully prepared response for prefix '{Prefix}' with seed {Seed}", prefix, extractedSeed);
                return Ok(response);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "GeoLin API error when fetching data for prefix '{Prefix}'. Status: {StatusCode}, Content: {Content}", 
                    prefix, apiEx.StatusCode, apiEx.Content);
                
                return StatusCode(500, new GeolinProblemDataResponse 
                { 
                    Error = $"GeoLin API error: {apiEx.Message}. Content: {apiEx.Content}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching data for prefix '{Prefix}' from GeoLin.", prefix);
                return StatusCode(500, new GeolinProblemDataResponse 
                { 
                    Error = $"An unexpected error occurred: {ex.Message}" 
                });
            }
        }
    }
} 