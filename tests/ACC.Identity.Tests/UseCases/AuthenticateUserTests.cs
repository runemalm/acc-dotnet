using ACC.BuildingBlocks.Failures;
using ACC.Identity.Application.UseCases.AuthenticateUser;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Tests.TestKit;
using Xunit;

namespace ACC.Identity.Tests.UseCases;

public sealed class AuthenticateUserTests
{
    [Fact]
    public void GivenActiveUserAndCorrectPassword_WhenAuthenticating_ThenAuthenticationTokenIssued()
    {
        var context = new IdentityUseCaseTestContext();
        var authenticatedAt = DateTimeOffset.UtcNow;
        var registered = context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            authenticatedAt);

        var result = context.AuthenticateUser.Handle(
            new AuthenticateUserCommand(" USER@example.com ", "correct horse battery staple"),
            authenticatedAt);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal(authenticatedAt.AddMinutes(60), result.ExpiresAt);
        Assert.NotNull(context.TokenIssuer.LastIssuedFor);
        Assert.Equal(registered.UserId, context.TokenIssuer.LastIssuedFor.UserId);
        Assert.Equal("user@example.com", context.TokenIssuer.LastIssuedFor.Email);
    }

    [Fact]
    public void GivenUnknownEmail_WhenAuthenticating_ThenAuthenticationFails()
    {
        var context = new IdentityUseCaseTestContext();

        var exception = Assert.Throws<AuthenticationFailedException>(() =>
            context.AuthenticateUser.Handle(
                new AuthenticateUserCommand("missing@example.com", "correct horse battery staple"),
                DateTimeOffset.UtcNow));

        Assert.Equal("Authentication must be valid.", exception.Message);
    }

    [Fact]
    public void GivenIncorrectPassword_WhenAuthenticating_ThenAuthenticationFails()
    {
        var context = new IdentityUseCaseTestContext();

        context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<AuthenticationFailedException>(() =>
            context.AuthenticateUser.Handle(
                new AuthenticateUserCommand("user@example.com", "wrong password"),
                DateTimeOffset.UtcNow));

        Assert.Equal("Authentication must be valid.", exception.Message);
    }
}
