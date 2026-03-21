using FluentAssertions;
using MathLLMBackend.Core.Services.ProblemsService;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace MathLLMBackend.Tests.Services;

public class ProblemsServiceTests
{
    private readonly AppDbContext _context;
    private readonly ProblemsService _service;

    public ProblemsServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);
        _service = new ProblemsService(_context);
    }

    [Fact]
    public async Task GetProblems_ReturnsAllProblems()
    {
        // Arrange
        var p1 = new Problem("sol1", "stmt1", "title1") { TheoryLink = "link1" };
        var p2 = new Problem("sol2", "stmt2", "title2") { TheoryLink = "link2" };
        _context.Problems.AddRange(p1, p2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProblems(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Title == "title1");
        result.Should().Contain(p => p.Title == "title2");
    }

    [Fact]
    public async Task GetProblemsByType_ReturnsOnlyCorrectType()
    {
        // Arrange
        var p1 = new Problem("sol1", "stmt1", "title1") { TheoryLink = "link1" };
        var p2 = new Problem("sol2", "stmt2", "title2") { TheoryLink = "link2" };
        _context.Problems.AddRange(p1, p2);
        
        var pt1 = new ProblemTaskType(p1, TaskType.Learning);
        var pt2 = new ProblemTaskType(p2, TaskType.Exam);
        _context.Set<ProblemTaskType>().AddRange(pt1, pt2);
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProblemsByType(TaskType.Learning, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("title1");
    }

    [Fact]
    public async Task GetProblem_ReturnsCorrectProblem()
    {
        // Arrange
        var p1 = new Problem("sol1", "stmt1", "title1") { TheoryLink = "link1" };
        _context.Problems.Add(p1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProblem(p1.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("title1");
    }

    [Fact]
    public async Task CreateProblem_AddsProblemToDb()
    {
        // Arrange
        var model = new ProblemUpdateModel("title", "stmt", "sol", "link", null, null, [TaskType.Learning]);

        // Act
        var result = await _service.CreateProblem(model, CancellationToken.None);

        // Assert
        var dbProblem = await _context.Problems.Include(p => p.Types).FirstAsync(p => p.Id == result.Id);
        dbProblem.Should().NotBeNull();
        dbProblem.Title.Should().Be("title");
        dbProblem.Types.Should().HaveCount(1);
        dbProblem.Types.First().TaskType.Should().Be(TaskType.Learning);
    }

    [Fact]
    public async Task CreateProblem_WithGeolinData_AddsGeolinData()
    {
        // Arrange
        var model = new ProblemUpdateModel("title", "stmt", "sol", "link", "hash123", 42, []);

        // Act
        var result = await _service.CreateProblem(model, CancellationToken.None);

        // Assert
        var dbProblem = await _context.Problems.Include(p => p.GeolinProblemData).FirstAsync(p => p.Id == result.Id);
        dbProblem.GeolinProblemData.Should().NotBeNull();
        dbProblem.GeolinProblemData.Hash.Should().Be("hash123");
        dbProblem.GeolinProblemData.Seed.Should().Be(42);
    }

    [Fact]
    public async Task UpdateProblem_UpdatesExistingProblem()
    {
        // Arrange
        var p = new Problem("sol", "stmt", "title") { TheoryLink = "link" };
        _context.Problems.Add(p);
        await _context.SaveChangesAsync();

        var model = new ProblemUpdateModel("new title", "new stmt", "new sol", "new link", null, null, []);

        // Act
        var result = await _service.UpdateProblem(p.Id, model, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("new title");
        result.Statement.Should().Be("new stmt");
    }

    [Fact]
    public async Task UpdateProblem_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var model = new ProblemUpdateModel("title", "stmt", "sol", null, null, null, []);

        // Act
        var result = await _service.UpdateProblem(Guid.NewGuid(), model, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProblem_UpdatesTypes()
    {
        // Arrange
        var p = new Problem("sol", "stmt", "title") { TheoryLink = "link" };
        _context.Problems.Add(p);
        var pt = new ProblemTaskType(p, TaskType.Learning);
        _context.ProblemTaskTypes.Add(pt);
        await _context.SaveChangesAsync();

        var model = new ProblemUpdateModel("title", "stmt", "sol", "link", null, null, [TaskType.Exam, TaskType.Guided]);

        // Act
        await _service.UpdateProblem(p.Id, model, CancellationToken.None);

        // Assert
        var dbProblem = await _context.Problems.Include(x => x.Types).FirstAsync(x => x.Id == p.Id);
        dbProblem.Types.Should().HaveCount(2);
        dbProblem.Types.Select(t => t.TaskType).Should().Contain(TaskType.Exam);
        dbProblem.Types.Select(t => t.TaskType).Should().Contain(TaskType.Guided);
        dbProblem.Types.Select(t => t.TaskType).Should().NotContain(TaskType.Learning);
    }

    [Fact]
    public async Task UpdateProblem_AddsGeolinData()
    {
        // Arrange
        var p = new Problem("sol", "stmt", "title") { TheoryLink = "link" };
        _context.Problems.Add(p);
        await _context.SaveChangesAsync();

        var model = new ProblemUpdateModel("title", "stmt", "sol", "link", "hash123", 42, []);

        // Act
        await _service.UpdateProblem(p.Id, model, CancellationToken.None);

        // Assert
        var dbProblem = await _context.Problems.Include(x => x.GeolinProblemData).FirstAsync(x => x.Id == p.Id);
        dbProblem.GeolinProblemData.Should().NotBeNull();
        dbProblem.GeolinProblemData.Hash.Should().Be("hash123");
        dbProblem.GeolinProblemData.Seed.Should().Be(42);
    }

    [Fact]
    public async Task UpdateProblem_RemovesGeolinData()
    {
        // Arrange
        var p = new Problem("sol", "stmt", "title") { TheoryLink = "link" };
        p.GeolinProblemData = new GeolinProblemData(p.Id, "oldhash", 1);
        _context.Problems.Add(p);
        await _context.SaveChangesAsync();

        var model = new ProblemUpdateModel("title", "stmt", "sol", "link", null, null, []);

        // Act
        await _service.UpdateProblem(p.Id, model, CancellationToken.None);

        // Assert
        var dbProblem = await _context.Problems.Include(x => x.GeolinProblemData).FirstAsync(x => x.Id == p.Id);
        dbProblem.GeolinProblemData.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProblem_RemovesFromDb()
    {
        // Arrange
        var p = new Problem("sol", "stmt", "title") { TheoryLink = "link" };
        _context.Problems.Add(p);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteProblem(p.Id, CancellationToken.None);

        // Assert
        var dbProblem = await _context.Problems.FindAsync(p.Id);
        dbProblem.Should().BeNull();
    }
}
