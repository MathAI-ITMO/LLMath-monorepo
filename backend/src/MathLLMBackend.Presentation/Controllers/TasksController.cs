using MathLLMBackend.Core.Services.GeolinService;
using MathLLMBackend.Presentation.Dtos.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MathLLMBackend.Core.Services.ProblemsService;
namespace MathLLMBackend.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly IGeolinService _geolinService;
    private readonly IProblemsService _problemsService;

    public TasksController(IGeolinService geolinService, IProblemsService problemsService)
    {
        _geolinService = geolinService;
        _problemsService = problemsService;
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
    public async Task<IActionResult> SaveProblem(string name, string problemHash, [FromQuery] int variationCount = 1, CancellationToken ct = default)
    {
        var result = await _problemsService.SaveProblems(name, problemHash, variationCount, ct);
        return Ok(result);
    }

    [HttpGet("getSavedProblems")]
    [Authorize]
    public async Task<IActionResult> GetSavedProblems(CancellationToken ct = default)
    {
        var problems = await _problemsService.GetSavedProblems(ct);
        return Ok(problems);
    }

    [HttpGet("getSavedProblemsByNames")]
    [Authorize]
    public async Task<IActionResult> GetSavedProblemsByNames(string name, CancellationToken ct = default)
    {
        var problems = await _problemsService.GetSavedProblemsByNames(name, ct);
        if (problems.Count == 0)
        {
            return NotFound();
        }
        return Ok(problems);     
    }

    [HttpGet("getAllNames")]
    [Authorize]
    public async Task<IActionResult> GetAllNames(CancellationToken ct = default)
    {
        var names = await _problemsService.GetAllTypes(ct);
        return Ok(names);
    }    
} 