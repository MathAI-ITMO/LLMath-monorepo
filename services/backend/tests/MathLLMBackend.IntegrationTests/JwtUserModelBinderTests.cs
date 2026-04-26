using System.Security.Claims;
using FluentAssertions;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Presentation.Binders;
using MathLLMBackend.Presentation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace MathLLMBackend.IntegrationTests;

public class JwtUserModelBinderTests
{
    private readonly JwtUserModelBinder _binder = new();

    private static ModelBindingContext MakeContext(ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext { User = principal };
        var mockCtx = new Mock<ModelBindingContext>();
        mockCtx.Setup(c => c.HttpContext).Returns(httpContext);
        mockCtx.Setup(c => c.ModelName).Returns("jwtUser");
        mockCtx.Setup(c => c.ModelState).Returns(new ModelStateDictionary());
        mockCtx.SetupProperty(c => c.Result);
        return mockCtx.Object;
    }

    [Fact]
    public async Task BindModelAsync_MissingNameIdentifier_SetsFailedResult()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, "a@b.com")]));
        var ctx = MakeContext(principal);

        await _binder.BindModelAsync(ctx);

        ctx.Result.IsModelSet.Should().BeFalse();
        ctx.ModelState.Should().ContainKey("jwtUser");
    }

    [Fact]
    public async Task BindModelAsync_AllClaimsPresent_BindsJwtUserCorrectly()
    {
        const string userId = "user-id-123";
        const string email = "user@example.com";
        const string firstName = "Ivan";
        const string lastName = "Petrov";
        const string group = "CS-42";
        const string role = "Admin";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypeConstants.FirstName, firstName),
            new Claim(ClaimTypeConstants.LastName, lastName),
            new Claim(ClaimTypeConstants.StudentGroup, group),
            new Claim(ClaimTypes.Role, role)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var ctx = MakeContext(principal);

        await _binder.BindModelAsync(ctx);

        ctx.Result.IsModelSet.Should().BeTrue();
        var jwtUser = ctx.Result.Model.Should().BeOfType<JwtUser>().Subject;
        jwtUser.Id.Should().Be(userId);
        jwtUser.Email.Should().Be(email);
        jwtUser.FirstName.Should().Be(firstName);
        jwtUser.LastName.Should().Be(lastName);
        jwtUser.StudentGroup.Should().Be(group);
        jwtUser.Role.Should().Be(role);
    }
}
