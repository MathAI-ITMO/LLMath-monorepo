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
}

public class SolveProblemRequest
{
    public string ProblemDescription { get; set; } = "";
}

public class SolveProblemResponse
{
    public string Solution { get; set; } = "";
} 