using MathLLMBackend.Core.Services.GeolinService;
using MathLLMBackend.Presentation.Dtos.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MathLLMBackend.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly IGeolinService _geolinService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IGeolinService geolinService, ILogger<TasksController> logger)
    {
        _geolinService = geolinService;
        _logger = logger;
    }

    [HttpGet("problems")]
    [Authorize]
    public async Task<IActionResult> GetProblems([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? prefixName = "", CancellationToken ct = default)
    {
        var response = await _geolinService.GetProblems(page, size, prefixName, ct);
        
        var problems = response.Problems.Select(p => new ProblemDto(
            Hash: p.Hash,
            Name: p.Name,
            Description: p.Description,
            Condition: p.ConditionRu
        )).ToList();

        var result = new ProblemsPageDto(
            Problems: problems,
            Number: response.Number
        );

        return Ok(result);
    }
} 