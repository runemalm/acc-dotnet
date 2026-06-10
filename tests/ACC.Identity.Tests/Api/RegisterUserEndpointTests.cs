using System.Net;
using System.Net.Http.Json;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Identity.Tests.Api;

public sealed class RegisterUserEndpointTests
{
    [Fact]
    public async Task RegisterUser_WithValidRequest_ReturnsCreated()
    {
        await using var context = await IdentityApiTestContext.Create();

        var response = await context.Client.PostAsJsonAsync(
            "/identity/register",
            new RegisterUserCommand("user@example.com", "correct horse battery staple"));

        var result = await response.Content.ReadFromJsonAsync<RegisterUserResult>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal(
            new Uri($"/identity/users/{result.UserId}", UriKind.Relative),
            response.Headers.Location);
        var email = Assert.Single(context.EmailSender.SentVerificationEmails);
        Assert.Equal("user@example.com", email.Email);
    }

    [Fact]
    public async Task RegisterUser_WithInvalidEmail_ReturnsBadRequest()
    {
        await using var context = await IdentityApiTestContext.Create();

        var response = await context.Client.PostAsJsonAsync(
            "/identity/register",
            new RegisterUserCommand("not-an-email-address", "correct horse battery staple"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Equal("A user email address must be valid.", problem.Detail);
    }
}
