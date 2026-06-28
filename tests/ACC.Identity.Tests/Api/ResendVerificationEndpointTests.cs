using System.Net;
using System.Net.Http.Json;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.ResendVerification;
using ACC.Identity.Tests.TestKit;
using Xunit;

namespace ACC.Identity.Tests.Api;

public sealed class ResendVerificationEndpointTests
{
    [Fact]
    public async Task ResendVerification_WithKnownEmail_ReturnsOk()
    {
        await using var context = await IdentityApiTestContext.Create();

        await context.Client.PostAsJsonAsync(
            "/identity/register",
            new RegisterUserCommand("user@example.com", "correct horse battery staple"));

        var response = await context.Client.PostAsJsonAsync(
            "/identity/resend-verification",
            new ResendVerificationCommand("user@example.com"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, context.EmailSender.SentVerificationEmails.Count);
    }

    [Fact]
    public async Task ResendVerification_WithUnknownEmail_ReturnsOk()
    {
        await using var context = await IdentityApiTestContext.Create();

        var response = await context.Client.PostAsJsonAsync(
            "/identity/resend-verification",
            new ResendVerificationCommand("missing@example.com"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(context.EmailSender.SentVerificationEmails);
    }
}
