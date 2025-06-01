using MathLLMBackend.Core.Services.LlmService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LlmController : ControllerBase
{
    private readonly ILlmService _llmService;
    private readonly ILogger<LlmController> _logger;

    public LlmController(ILlmService llmService, ILogger<LlmController> logger)
    {
        _llmService = llmService;
        _logger = logger;
    }

    /// <summary>
    /// Решает математическую задачу с помощью LLM
    /// </summary>
    [HttpPost("solve-problem")]
    public async Task<IActionResult> SolveProblem([FromBody] SolveProblemRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProblemDescription))
        {
            return BadRequest("Problem description cannot be empty");
        }

        try
        {
            _logger.LogInformation("Solving problem using LLM: {Problem}", request.ProblemDescription);
            var solution = await _llmService.SolveProblem(request.ProblemDescription, ct);
            return Ok(new SolveProblemResponse { Solution = solution });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error solving problem with LLM");
            return StatusCode(500, "Error solving problem: " + ex.Message);
        }
    }

    /// <summary>
    /// Извлекает финальный ответ из готового решения задачи
    /// </summary>
    [HttpPost("extract-answer")]
    public async Task<IActionResult> ExtractAnswer([FromBody] ExtractAnswerRequest request, CancellationToken ct)
    {
        _logger.LogInformation("ExtractAnswer called with ProblemStatement length: {ProblemStatementLength}, Solution length: {SolutionLength}", 
            request.ProblemStatement?.Length ?? 0, request.Solution?.Length ?? 0);
        
        if (string.IsNullOrWhiteSpace(request.ProblemStatement))
        {
            _logger.LogWarning("ExtractAnswer: Problem statement is empty");
            return BadRequest("Problem statement cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(request.Solution))
        {
            _logger.LogWarning("ExtractAnswer: Solution is empty");
            return BadRequest("Solution cannot be empty");
        }

        try
        {
            _logger.LogInformation("Extracting answer from solution for problem. ProblemStatement preview: {ProblemPreview}", 
                request.ProblemStatement.Substring(0, Math.Min(100, request.ProblemStatement.Length)));
            _logger.LogInformation("Solution preview: {SolutionPreview}", 
                request.Solution.Substring(0, Math.Min(200, request.Solution.Length)));
                
            var extractedAnswer = await _llmService.ExtractAnswer(request.ProblemStatement, request.Solution, ct);
            
            _logger.LogInformation("Successfully extracted answer: {ExtractedAnswer}", extractedAnswer);
            return Ok(new ExtractAnswerResponse { ExtractedAnswer = extractedAnswer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting answer from solution. Exception type: {ExceptionType}, Message: {ExceptionMessage}", 
                ex.GetType().Name, ex.Message);
            return StatusCode(500, "Error extracting answer: " + ex.Message);
        }
    }
}

public class SolveProblemRequest
{
    public string ProblemDescription { get; set; } = "";
}

public class SolveProblemResponse
{
    public string Solution { get; set; } = "";
}

public class ExtractAnswerRequest
{
    public string ProblemStatement { get; set; } = "";
    public string Solution { get; set; } = "";
}

public class ExtractAnswerResponse
{
    public string ExtractedAnswer { get; set; } = "";
} 