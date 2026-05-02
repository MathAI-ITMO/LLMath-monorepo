using FluentAssertions;
using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
using Moq;
using Xunit;

namespace MathLLMBackend.Tests.Services;

public class GeolinServiceTests
{
    private readonly Mock<IGeolinApi> _geolinApiMock;
    private readonly GeolinService _service;

    public GeolinServiceTests()
    {
        _geolinApiMock = new Mock<IGeolinApi>();
        _service = new GeolinService(_geolinApiMock.Object);
    }

    [Fact]
    public async Task GetProblem_PrefixNotFound_ThrowsInvalidOperationException()
    {
        _geolinApiMock
            .Setup(x => x.GetProblemsInfo(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemPageResponse { Problems = [] });

        var act = () => _service.GetProblem("nonexistent");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nonexistent*");
    }

    [Fact]
    public async Task GetProblem_ConditionResponseNull_ThrowsInvalidOperationException()
    {
        var problemInfo = new ProblemInfoResponse { Name = "Test", Hash = "hash123" };
        _geolinApiMock
            .Setup(x => x.GetProblemsInfo(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemPageResponse { Problems = [problemInfo] });
        _geolinApiMock
            .Setup(x => x.GetProblemCondition(It.IsAny<ProblemConditionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProblemConditionResponse)null!);

        var act = () => _service.GetProblem("prefix");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*GeoLin*");
    }

    [Fact]
    public async Task GetProblem_WithNoSeed_GeneratesRandomSeed()
    {
        var problemInfo = new ProblemInfoResponse { Name = "Test", Hash = "hash123" };
        _geolinApiMock
            .Setup(x => x.GetProblemsInfo(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemPageResponse { Problems = [problemInfo] });
        _geolinApiMock
            .Setup(x => x.GetProblemCondition(It.IsAny<ProblemConditionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemConditionResponse { Condition = "test condition", ProblemParams = "params" });

        var result = await _service.GetProblem("prefix");

        result.Seed.Should().BeInRange(1, 1_000_000_000);
    }

    [Fact]
    public async Task GetProblem_WithExplicitSeed_UsesThatSeed()
    {
        const int seed = 42;
        var problemInfo = new ProblemInfoResponse { Name = "Test", Hash = "hash123" };
        _geolinApiMock
            .Setup(x => x.GetProblemsInfo(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemPageResponse { Problems = [problemInfo] });

        ProblemConditionRequest? captured = null;
        _geolinApiMock
            .Setup(x => x.GetProblemCondition(It.IsAny<ProblemConditionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ProblemConditionRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new ProblemConditionResponse { Condition = "test condition", ProblemParams = "params" });

        var result = await _service.GetProblem("prefix", seed: seed);

        captured!.Seed.Should().Be(seed);
        result.Seed.Should().Be(seed);
    }

    [Fact]
    public async Task CheckAnswer_VerdictAtThreshold_ReturnsIsCorrectTrue()
    {
        _geolinApiMock
            .Setup(x => x.CheckProblemAnswer(It.IsAny<ProblemAnswerCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemAnswerCheckResponse { Verdict = 1.0 });

        var result = await _service.CheckAnswer("hash", "42", 1, "params");

        result.IsCorrect.Should().BeTrue();
        result.Verdict.Should().Be(1.0);
    }

    [Fact]
    public async Task CheckAnswer_VerdictBelowThreshold_ReturnsIsCorrectFalse()
    {
        _geolinApiMock
            .Setup(x => x.CheckProblemAnswer(It.IsAny<ProblemAnswerCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemAnswerCheckResponse { Verdict = 0.5 });

        var result = await _service.CheckAnswer("hash", "wrong", 1, "params");

        result.IsCorrect.Should().BeFalse();
        result.Verdict.Should().Be(0.5);
    }
}
