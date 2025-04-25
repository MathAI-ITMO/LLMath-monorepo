using MathLLMBackend.Core.Services.GeolinService;
using MathLLMBackend.Presentation.Dtos.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
namespace MathLLMBackend.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly IGeolinService _geolinService;
    private readonly IGeolinApi _geolinApi;
    private readonly ILogger<TasksController> _logger;  

    public TasksController(IGeolinService geolinService, ILogger<TasksController> logger, IGeolinApi geolinApi)
    {
        _geolinService = geolinService;
        _logger = logger;
        _geolinApi = geolinApi;
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

    [HttpPost("saveProblem")]
    [Authorize]
    public async Task<IActionResult> SaveProblem(string name, string problemHash, [FromQuery] int variationCount = 10, CancellationToken ct = default)
    {
        var problem = await _geolinApi.GetProblemCondition(
            new ProblemConditionRequest()
            {
                Hash = problemHash,
                Seed = new Random().Next(),
                Lang = "ru"
            });
        
        
    
        return Ok();
    }
} 