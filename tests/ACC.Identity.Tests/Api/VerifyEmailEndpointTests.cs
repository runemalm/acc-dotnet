using System.Net;
using System.Net.Http.Json;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.VerifyEmail;
using ACC.Identity.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Identity.Tests.Api;

public sealed class VerifyEmailEndpointTests
{
    [Fact]
    public async Task VerifyEmail_WithValidToken_ReturnsOk()
    {
        await using var context = await IdentityApiTestContext.Create();

        await context.Client.PostAsJsonAsync(
            "/identity/register",
            new RegisterUserCommand("user@example.com", "correct horse battery staple"));

        var token = Assert.Single(context.EmailSender.SentVerificationEmails).VerificationToken;

        var response = await context.Client.PostAsJsonAsync(
            "/identity/verify-email",
            new VerifyEmailCommand(token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidToken_ReturnsBadRequest()
    {
        await using var context = await IdentityApiTestContext.Create();

        var response = await context.Client.PostAsJsonAsync(
            "/identity/verify-email",
            new VerifyEmailCommand("not-the-token"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Equal("Email verification must be valid.", problem.Detail);
    }
}
