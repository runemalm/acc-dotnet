using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Domain.Invariants;
using ACC.Identity.Tests.TestKit;
using Xunit;

namespace ACC.Identity.Tests.UseCases;

public sealed class RegisterUserTests
{
    [Fact]
    public void GivenNewEmail_WhenRegisteringUser_ThenUserRegistered()
    {
        var context = new IdentityUseCaseTestContext();

        var result = context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            DateTimeOffset.UtcNow);

        var user = context.FindUser(result.UserId);

        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.NotNull(user);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("USER@EXAMPLE.COM", user.NormalizedEmail);
        Assert.True(user.IsActive);
        Assert.Null(user.EmailVerifiedAt);
        Assert.Equal("access-token", result.AccessToken);
        var email = Assert.Single(context.EmailSender.SentVerificationEmails);
        Assert.Equal("user@example.com", email.Email);
        Assert.Equal("verification-token-1", email.VerificationToken);
        Assert.NotNull(context.TokenIssuer.LastIssuedFor);
        Assert.Equal(result.UserId, context.TokenIssuer.LastIssuedFor.UserId);
        Assert.Equal("user@example.com", context.TokenIssuer.LastIssuedFor.Email);
    }

    [Fact]
    public void GivenExistingEmail_WhenRegisteringUser_ThenUserEmailMustBeUniqueViolation()
    {
        var context = new IdentityUseCaseTestContext();

        context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<UserEmailMustBeUniqueViolation>(() =>
            context.RegisterUser.Handle(
                new RegisterUserCommand("USER@example.com", "another password"),
                DateTimeOffset.UtcNow));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void GivenInvalidEmail_WhenRegisteringUser_ThenUserEmailMustBeValidViolation()
    {
        var context = new IdentityUseCaseTestContext();

        var exception = Assert.Throws<UserEmailMustBeValidViolation>(() =>
            context.RegisterUser.Handle(
                new RegisterUserCommand("not-an-email-address", "correct horse battery staple"),
                DateTimeOffset.UtcNow));

        Assert.Equal("A user email address must be valid.", exception.Message);
    }

}
