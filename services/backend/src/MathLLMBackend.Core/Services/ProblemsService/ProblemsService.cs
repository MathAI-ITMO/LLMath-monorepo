using MathLLMBackend.Core.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MathLLMBackend.Core.Services.ProblemsService;

public class ProblemsService(IAppDbContext dbContext) : IProblemsService
{
    private readonly IAppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<Problem>> GetProblems(CancellationToken ct)
    {
        return await _dbContext.Problems
            .Include(p => p.Types)
            .Include(p => p.GeolinProblemData)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Problem>> GetProblemsByType(TaskType taskType, CancellationToken ct)
    {
        return await _dbContext.Problems
            .Include(p => p.Types)
            .Include(p => p.GeolinProblemData)
            .Where(p => p.Types.Any(t => t.TaskType == taskType))
            .ToListAsync(ct);
    }

    public async Task<Problem?> GetProblem(Guid problemId, CancellationToken ct)
    {
        return await _dbContext.Problems
            .Include(p => p.Types)
            .Include(p => p.GeolinProblemData)
            .FirstOrDefaultAsync(p => p.Id == problemId, ct);
    }

    public async Task<Problem?> UpdateProblem(Guid problemId, ProblemUpdateModel model, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var problem = await _dbContext.Problems
            .Include(p => p.Types)
            .Include(p => p.GeolinProblemData)
            .FirstOrDefaultAsync(p => p.Id == problemId, ct);

        if (problem == null)
        {
            return null;
        }

        problem.Title = model.Title;
        problem.Statement = model.Statement;
        problem.LlmSolution = model.LlmSolution;
        problem.TheoryLink = model.TheoryLink;

        UpdateGeolinData(problem, model);
        UpdateTypes(problem, model.Types);

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return problem;
    }

    public async Task<Problem> CreateProblem(ProblemUpdateModel model, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var problem = new Problem
        {
            Title = model.Title,
            Statement = model.Statement,
            LlmSolution = model.LlmSolution,
            TheoryLink = model.TheoryLink
        };

        if (!string.IsNullOrEmpty(model.GeolinHash))
        {
            problem.GeolinProblemData = new GeolinProblemData(problem.Id, model.GeolinHash, model.GeolinSeed ?? 0);
        }

        _dbContext.Problems.Add(problem);

        foreach (var type in model.Types)
        {
            var problemTaskType = new ProblemTaskType(problem, type);
            _dbContext.ProblemTaskTypes.Add(problemTaskType);
        }

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return problem;
    }

    public async Task DeleteProblem(Guid problemId, CancellationToken ct)
    {
        var problem = await _dbContext.Problems.FindAsync([problemId], ct);
        if (problem != null)
        {
            _dbContext.Problems.Remove(problem);
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    private void UpdateGeolinData(Problem problem, ProblemUpdateModel model)
    {
        if (!string.IsNullOrEmpty(model.GeolinHash))
        {
            if (problem.GeolinProblemData == null)
            {
                var geolinData = new GeolinProblemData(problem.Id, model.GeolinHash, model.GeolinSeed ?? 0);
                _dbContext.GeolinProblems.Add(geolinData);
                problem.GeolinProblemData = geolinData;
            }
            else
            {
                problem.GeolinProblemData.Hash = model.GeolinHash;
                problem.GeolinProblemData.Seed = model.GeolinSeed ?? 0;
            }
        }
        else if (problem.GeolinProblemData != null)
        {
            _dbContext.GeolinProblems.Remove(problem.GeolinProblemData);
            problem.GeolinProblemData = null!;
        }
    }

    private void UpdateTypes(Problem problem, IEnumerable<TaskType> newTypes)
    {
        var newTypesSet = newTypes.ToHashSet();
        var existingTypes = problem.Types.Select(t => t.TaskType).ToHashSet();

        // Remove types that are no longer present
        var typesToRemove = problem.Types.Where(t => !newTypesSet.Contains(t.TaskType)).ToList();
        foreach (var type in typesToRemove)
        {
            problem.Types.Remove(type);
            _dbContext.ProblemTaskTypes.Remove(type);
        }

        // Add new types
        foreach (var type in newTypesSet.Except(existingTypes))
        {
            var problemTaskType = new ProblemTaskType(problem, type);
            problem.Types.Add(problemTaskType);
        }
    }
}
