using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Presentation.Dtos.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MathLLMBackend.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected ApplicationUser? TestUser;
    protected HttpClient? AuthenticatedClient;

    protected BaseIntegrationTest(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<ApplicationUser> CreateAndLoginUserAsync(string email = "test@example.com", string password = "Test123!@#")
    {
        var user = await Factory.CreateTestUserAsync(email, password);
        TestUser = user;
        AuthenticatedClient = Factory.CreateAuthenticatedClient(user);
        return user;
    }

    protected async Task<ApplicationUser> CreateAndLoginAdminUserAsync(string email = "admin@example.com", string password = "Test123!@#")
    {
        var user = await Factory.CreateTestAdminUserAsync(email, password);
        TestUser = user;
        AuthenticatedClient = Factory.CreateAuthenticatedClient(user);
        return user;
    }

    protected async Task<HttpResponseMessage> AuthenticatedGetAsync(string url)
    {
        AuthenticatedClient ??= await CreateAuthenticatedClientAsync();
        return await AuthenticatedClient.GetAsync(url);
    }

    protected async Task<HttpResponseMessage> AuthenticatedPostAsync(string url, object? content = null)
    {
        AuthenticatedClient ??= await CreateAuthenticatedClientAsync();
        if (content != null)
        {
            return await AuthenticatedClient.PostAsJsonAsync(url, content);
        }
        return await AuthenticatedClient.PostAsync(url, null);
    }

    protected async Task<HttpResponseMessage> AuthenticatedDeleteAsync(string url)
    {
        AuthenticatedClient ??= await CreateAuthenticatedClientAsync();
        return await AuthenticatedClient.DeleteAsync(url);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        if (TestUser == null)
        {
            await CreateAndLoginUserAsync();
        }

        return AuthenticatedClient!;
    }

    public void Dispose()
    {
        Client?.Dispose();
        AuthenticatedClient?.Dispose();
    }
}
