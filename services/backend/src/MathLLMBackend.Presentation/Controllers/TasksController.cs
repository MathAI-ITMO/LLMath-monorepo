using MathLLMBackend.Core.Services.GeolinService;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Presentation.Dtos.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MathLLMBackend.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly IGeolinService _geolinService;

    public TasksController(IGeolinService geolinService)
    {
        _geolinService = geolinService;
    }

    [HttpGet("problem/{prefixName}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> GetProblemByPrefix(
        [FromRoute] string prefixName,
        [FromQuery] int? seed,
        CancellationToken ct = default)
    {
        var response = await _geolinService.GetProblem(prefixName, seed, ct);

        var result = new ProblemDto(
            Hash: response.Hash,
            Name: response.Name,
            Description: response.Description,
            Condition: response.Condition,
            Seed: response.Seed,
            Params: response.ProblemParams
        );

        return Ok(result);
    }
}