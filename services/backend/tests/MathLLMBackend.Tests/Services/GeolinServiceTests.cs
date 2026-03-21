using FluentAssertions;
using MathLLMBackend.Core.Services.GeolinService;
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
}
