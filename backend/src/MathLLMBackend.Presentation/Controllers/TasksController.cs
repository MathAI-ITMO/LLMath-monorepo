using MathLLMBackend.Core.Services.GeolinService;
using MathLLMBackend.Presentation.Dtos.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
using MathLLMBackend.ProblemsClient;
using MathLLMBackend.ProblemsClient.Models;
using System.Linq.Expressions;
namespace MathLLMBackend.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly IGeolinService _geolinService;
    private readonly IGeolinApi _geolinApi;
    private readonly IProblemsAPI _problemsAPI;
    private readonly ILogger<TasksController> _logger;  

    public TasksController(IGeolinService geolinService, ILogger<TasksController> logger, IGeolinApi geolinApi, IProblemsAPI problemsAPI)
    {
        _geolinService = geolinService;
        _logger = logger;
        _geolinApi = geolinApi;
        _problemsAPI = problemsAPI;
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
        List <Problem> result = new();
        for (int i = 0; i < variationCount; ++i)
        {    
            int seed = new Random().Next();
            var problem = await _geolinApi.GetProblemCondition(
                new ProblemConditionRequest()
                {
                    Hash = problemHash,
                    Seed = seed,
                    Lang = "ru"
                });
            var problemMongo = new ProblemRequest()
            {
                Statement = problem.Condition,
                GeolinAnsKey = new GeolinKey()
                {
                    Hash = problemHash,
                    Seed = seed
                }
            };
            var createdProblem = await _problemsAPI.CreateProblem(problemMongo);
            result.Add(createdProblem);
            var tmp = await _problemsAPI.GiveANameProblem(new ProblemWithNameRequest()
                {
                    Name = name, 
                    ProblemId = createdProblem.Id
                }
            );
        }      
        return Ok(result);
    }

    [HttpGet("getSavedProblems")]
    [Authorize]
    public async Task<IActionResult> GetSavedProblems(CancellationToken ct = default)
    {
        var problems = await _problemsAPI.GetProblems();
        return Ok(problems);
    }

    [HttpGet("getSavedProblemsByNames")]
    [Authorize]
    public async Task<IActionResult> GetSavedProblemsByNames(string name, CancellationToken ct = default)
    {
        try
        {
            var problems = await _problemsAPI.GetAllProblemsByName(name);
            return Ok(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching problems from external problems service {message}", ex.Message);
            return NotFound();
        }        
    }
} 