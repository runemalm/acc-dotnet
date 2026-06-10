using System.Net;
using System.Net.Http.Json;
using ACC.Identity.Application.UseCases.AuthenticateUser;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Identity.Tests.Api;

public sealed class AuthenticateUserEndpointTests
{
    [Fact]
    public async Task AuthenticateUser_WithValidCredentials_ReturnsOk()
    {
        await using var context = await IdentityApiTestContext.Create();

        await context.Client.PostAsJsonAsync(
            "/identity/register",
            new RegisterUserCommand("user@example.com", "correct horse battery staple"));

        var response = await context.Client.PostAsJsonAsync(
            "/identity/authenticate",
            new AuthenticateUserCommand("user@example.com", "correct horse battery staple"));

        var result = await response.Content.ReadFromJsonAsync<AuthenticateUserResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
    }

    [Fact]
    public async Task AuthenticateUser_WithInvalidCredentials_ReturnsBadRequest()
    {
        await using var context = await IdentityApiTestContext.Create();

        var response = await context.Client.PostAsJsonAsync(
            "/identity/authenticate",
            new AuthenticateUserCommand("missing@example.com", "correct horse battery staple"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Equal("Authentication must be valid.", problem.Detail);
    }
}
