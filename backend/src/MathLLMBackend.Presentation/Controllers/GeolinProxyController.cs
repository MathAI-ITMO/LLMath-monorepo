using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Linq;
using System.Net.Http;
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

        /// <summary>
        /// Проверяет ответ на задачу через GeoLin API
        /// </summary>
        [HttpPost("check-answer")]
        public async Task<IActionResult> CheckAnswer([FromBody] CheckAnswerRequest request)
        {
            _logger.LogInformation("CheckAnswer called with Hash: {Hash}, AnswerAttempt: {AnswerAttempt}, Seed: {Seed}, ProblemParams length: {ProblemParamsLength}",
                request.Hash, request.AnswerAttempt, request.Seed, request.ProblemParams?.Length ?? 0);
                
            if (string.IsNullOrWhiteSpace(request.Hash))
            {
                _logger.LogWarning("CheckAnswer: Hash is empty");
                return BadRequest(new CheckAnswerResponse 
                { 
                    Error = "Hash is required and cannot be empty." 
                });
            }

            if (string.IsNullOrWhiteSpace(request.AnswerAttempt))
            {
                _logger.LogWarning("CheckAnswer: AnswerAttempt is empty");
                return BadRequest(new CheckAnswerResponse 
                { 
                    Error = "Answer attempt is required and cannot be empty." 
                });
            }

            try
            {
                _logger.LogInformation("Checking answer for hash '{Hash}' with seed {Seed}", request.Hash, request.Seed);

                var checkRequest = new ProblemAnswerCheckRequest()
                {
                    Hash = request.Hash,
                    AnswerAttempt = request.AnswerAttempt
                };

                // Добавляем seed если есть
                if (request.Seed.HasValue)
                {
                    checkRequest.Seed = request.Seed.Value;
                    _logger.LogInformation("Added seed {Seed} to GeoLin request", request.Seed.Value);
                }

                // Добавляем problem_params если есть  
                if (!string.IsNullOrWhiteSpace(request.ProblemParams))
                {
                    checkRequest.ProblemParams = request.ProblemParams;
                    _logger.LogInformation("Added ProblemParams to GeoLin request: {ProblemParams}", request.ProblemParams);
                }

                _logger.LogInformation("Sending request to GeoLin API: {Request}", System.Text.Json.JsonSerializer.Serialize(checkRequest));
                
                var checkResult = await _geolinApi.CheckProblemAnswer(checkRequest);
                
                _logger.LogInformation("Received response from GeoLin API: Verdict={Verdict}", checkResult.Verdict);

                // ProblemAnswerCheckResponse содержит только поле Verdict (double)
                // Преобразуем verdict в IsCorrect - считаем что verdict >= 1.0 означает правильный ответ
                bool isCorrect = checkResult.Verdict >= 1.0;
                string message = isCorrect 
                    ? $"Ответ правильный (verdict: {checkResult.Verdict})" 
                    : $"Ответ неправильный (verdict: {checkResult.Verdict})";

                var response = new CheckAnswerResponse
                {
                    IsCorrect = isCorrect,
                    Message = message,
                    Hash = request.Hash,
                    AnswerAttempt = request.AnswerAttempt,
                    Seed = request.Seed
                };

                _logger.LogInformation("Answer check completed for hash '{Hash}'. Result: {IsCorrect} (verdict: {Verdict})", request.Hash, isCorrect, checkResult.Verdict);
                return Ok(response);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "GeoLin API error when checking answer for hash '{Hash}'. Status: {StatusCode}, Content: {Content}", 
                    request.Hash, apiEx.StatusCode, apiEx.Content);
                
                return StatusCode(500, new CheckAnswerResponse 
                { 
                    Error = $"GeoLin API error: {apiEx.Message}. Content: {apiEx.Content}",
                    Hash = request.Hash,
                    AnswerAttempt = request.AnswerAttempt,
                    Seed = request.Seed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when checking answer for hash '{Hash}'. Exception type: {ExceptionType}, Message: {ExceptionMessage}", 
                    request.Hash, ex.GetType().Name, ex.Message);
                return StatusCode(500, new CheckAnswerResponse 
                { 
                    Error = $"An unexpected error occurred: {ex.Message}",
                    Hash = request.Hash,
                    AnswerAttempt = request.AnswerAttempt,
                    Seed = request.Seed
                });
            }
        }

        /// <summary>
        /// Проверяет ответ на задачу через прямой вызов GeoLin API (обходной путь)
        /// TODO: Эту функцию надо заменить после того как сервис LLMath-Problems научится верифицировать решения задач
        /// </summary>
        [HttpPost("check-answer-direct")]
        public async Task<IActionResult> CheckAnswerDirect([FromBody] CheckAnswerRequest request)
        {
            // Константы для прямого обращения к GeoLin API
            const string GEOLIN_BASE_URL = "https://geolin.dev.mgsds.com";
            const string GEOLIN_AUTH_HEADER = "Basic Z2VvbGluLXVzZXI6RmczNXRoaDI2a2ZO";
            
            _logger.LogInformation("CheckAnswerDirect called with Hash: {Hash}, AnswerAttempt: {AnswerAttempt}, Seed: {Seed}",
                request.Hash, request.AnswerAttempt, request.Seed);
                
            if (string.IsNullOrWhiteSpace(request.Hash))
            {
                return BadRequest(new CheckAnswerResponse { Error = "Hash is required and cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(request.AnswerAttempt))
            {
                return BadRequest(new CheckAnswerResponse { Error = "Answer attempt is required and cannot be empty." });
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", GEOLIN_AUTH_HEADER);

                var payload = new Dictionary<string, object>
                {
                    ["hash"] = request.Hash,
                    ["answer_attempt"] = request.AnswerAttempt
                };

                // Добавляем problem_params если есть, иначе seed
                if (!string.IsNullOrWhiteSpace(request.ProblemParams))
                {
                    // Проверяем, является ли problem_params уже JSON строкой
                    try
                    {
                        var testParse = JsonDocument.Parse(request.ProblemParams);
                        payload["problem_params"] = request.ProblemParams;
                        _logger.LogInformation("Added problem_params as JSON string: {ProblemParams}", request.ProblemParams);
                    }
                    catch (JsonException)
                    {
                        // Если не JSON, оборачиваем в кавычки
                        payload["problem_params"] = $"\"{request.ProblemParams}\"";
                        _logger.LogInformation("Added problem_params as quoted string: {ProblemParams}", payload["problem_params"]);
                    }
                }
                else if (request.Seed.HasValue)
                {
                    payload["seed"] = request.Seed.Value;
                    _logger.LogInformation("Added seed: {Seed}", request.Seed.Value);
                }

                var jsonPayload = JsonSerializer.Serialize(payload);
                _logger.LogInformation("Sending direct request to GeoLin: {Url}, Payload: {Payload}", 
                    $"{GEOLIN_BASE_URL}/problem-answer-check", jsonPayload);

                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{GEOLIN_BASE_URL}/problem-answer-check", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("GeoLin response: Status={Status}, Content={Content}", 
                    response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GeoLin API returned error status: {Status}, Content: {Content}", 
                        response.StatusCode, responseContent);
                    
                    return StatusCode(500, new CheckAnswerResponse 
                    { 
                        Error = $"GeoLin API error: {response.StatusCode} - {responseContent}",
                        Hash = request.Hash,
                        AnswerAttempt = request.AnswerAttempt,
                        Seed = request.Seed
                    });
                }

                // Парсим ответ от GeoLin
                var geolinResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // Извлекаем verdict
                if (!geolinResponse.TryGetProperty("verdict", out var verdictElement))
                {
                    _logger.LogError("GeoLin response does not contain 'verdict' field: {Response}", responseContent);
                    return StatusCode(500, new CheckAnswerResponse 
                    { 
                        Error = "Invalid response from GeoLin API: missing verdict field",
                        Hash = request.Hash,
                        AnswerAttempt = request.AnswerAttempt,
                        Seed = request.Seed
                    });
                }

                var verdict = verdictElement.GetDouble();
                bool isCorrect = verdict >= 1.0;
                string message = isCorrect 
                    ? $"Ответ правильный (verdict: {verdict})" 
                    : $"Ответ неправильный (verdict: {verdict})";

                var checkResponse = new CheckAnswerResponse
                {
                    IsCorrect = isCorrect,
                    Message = message,
                    Hash = request.Hash,
                    AnswerAttempt = request.AnswerAttempt,
                    Seed = request.Seed
                };

                _logger.LogInformation("Answer check completed for hash '{Hash}'. Result: {IsCorrect} (verdict: {Verdict})", 
                    request.Hash, isCorrect, verdict);
                
                return Ok(checkResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in direct GeoLin API call for hash '{Hash}'. Exception: {ExceptionType}, Message: {ExceptionMessage}", 
                    request.Hash, ex.GetType().Name, ex.Message);
                
                return StatusCode(500, new CheckAnswerResponse 
                { 
                    Error = $"Error calling GeoLin API directly: {ex.Message}",
                    Hash = request.Hash,
                    AnswerAttempt = request.AnswerAttempt,
                    Seed = request.Seed
                });
            }
        }
    }

    public class CheckAnswerRequest
    {
        public string Hash { get; set; } = "";
        public string AnswerAttempt { get; set; } = "";
        public int? Seed { get; set; }
        public string? ProblemParams { get; set; }
    }

    public class CheckAnswerResponse
    {
        public bool IsCorrect { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public string Hash { get; set; } = "";
        public string AnswerAttempt { get; set; } = "";
        public int? Seed { get; set; }
    }
} 