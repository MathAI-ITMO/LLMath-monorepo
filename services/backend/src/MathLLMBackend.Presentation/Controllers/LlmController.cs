using MathLLMBackend.Core.Constants;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Presentation.Dtos.Llm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
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

    [HttpPost("solve-problem")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> SolveProblem([FromBody] SolveProblemRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProblemDescription))
        {
            return BadRequest("Problem description cannot be empty");
        }

        _logger.LogInformation("Solving problem using LLM: {Problem}", request.ProblemDescription);
        var solution = await _llmService.SolveProblem(request.ProblemDescription, ct);
        return Ok(new SolveProblemResponse { Solution = solution });
    }

    [HttpPost("extract-answer")]
    [Authorize(Roles = Role.Admin)]
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

        _logger.LogInformation("Extracting answer from solution for problem. ProblemStatement preview: {ProblemPreview}", 
            request.ProblemStatement.Substring(0, Math.Min(MessageConstants.Logging.MaxProblemPreviewLength, request.ProblemStatement.Length)));
        _logger.LogInformation("Solution preview: {SolutionPreview}", 
            request.Solution.Substring(0, Math.Min(MessageConstants.Logging.MaxSolutionPreviewLength, request.Solution.Length)));
            
        var extractedAnswer = await _llmService.ExtractAnswer(request.ProblemStatement, request.Solution, ct);
        
        _logger.LogInformation("Successfully extracted answer: {ExtractedAnswer}", extractedAnswer);
        return Ok(new ExtractAnswerResponse { ExtractedAnswer = extractedAnswer });
    }
}