using Xunit;

namespace MathLLMBackend.IntegrationTests;

[CollectionDefinition("Integration Tests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
}
