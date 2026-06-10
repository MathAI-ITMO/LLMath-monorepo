using MathLLMBackend.Core.Services.ProblemsService;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Models;
using MathLLMBackend.Presentation.Dtos.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers;

[Authorize(Roles = Role.Admin)]
[Route("api/[controller]")]
[ApiController]
public class ProblemsController(IProblemsService problemsService) : ControllerBase
{
    private readonly IProblemsService _problemsService = problemsService;

    [HttpGet]
    public async Task<IActionResult> GetProblems(CancellationToken ct)
    {
        var problems = await _problemsService.GetProblems(ct);
        return Ok(problems.Select(MapToDto));
    }

    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetProblemsByType(TaskType type, CancellationToken ct)
    {
        var problems = await _problemsService.GetProblemsByType(type, ct);
        return Ok(problems.Select(MapToDto));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProblem([FromBody] CreateProblemRequestDto dto, CancellationToken ct)
    {
        var model = MapToModel(dto);
        var problem = await _problemsService.CreateProblem(model, ct);
        return Ok(MapToDto(problem));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProblem(Guid id, [FromBody] UpdateProblemRequestDto dto, CancellationToken ct)
    {
        var model = MapToModel(dto);
        var problem = await _problemsService.UpdateProblem(id, model, ct);

        if (problem == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(problem));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProblem(Guid id, CancellationToken ct)
    {
        await _problemsService.DeleteProblem(id, ct);
        return NoContent();
    }

    private static ProblemUpdateModel MapToModel(CreateProblemRequestDto dto) =>
        new(dto.Title, dto.Statement, dto.LlmSolution, dto.TheoryLink, dto.GeolinHash, dto.GeolinSeed, dto.Types);

    private static ProblemUpdateModel MapToModel(UpdateProblemRequestDto dto) =>
        new(dto.Title, dto.Statement, dto.LlmSolution, dto.TheoryLink, dto.GeolinHash, dto.GeolinSeed, dto.Types);

    private static AdminProblemDto MapToDto(Problem problem)
    {
        return new AdminProblemDto(
            problem.Id,
            problem.Title,
            problem.Statement,
            problem.LlmSolution,
            problem.TheoryLink,
            problem.GeolinProblemData?.Hash,
            problem.GeolinProblemData?.Seed,
            problem.Types.Select(t => t.TaskType)
        );
    }
}
