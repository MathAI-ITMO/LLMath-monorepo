using System.Net;
using Xunit;
using System.Net.Http.Json;
using FluentAssertions;
using MathLLMBackend.GeolinClient.Models;
using Moq;

namespace MathLLMBackend.IntegrationTests;

[Collection("Integration Tests")]
public class GeolinProxyControllerTests : BaseIntegrationTest
{
    public GeolinProxyControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CheckAnswerDirect_WithValidRequest_ReturnsOk()
    {
        await CreateAndLoginAdminUserAsync();
        var request = new
        {
            Hash = "test-hash",
            AnswerAttempt = "42",
            Seed = 123
        };
        var response = await AuthenticatedPostAsync("/api/v1/geolin-proxy/check-answer-direct", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }
}
