using ACC.BuildingBlocks.Failures;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.VerifyEmail;
using ACC.Identity.Tests.TestKit;
using Xunit;

namespace ACC.Identity.Tests.UseCases;

public sealed class VerifyEmailTests
{
    [Fact]
    public void GivenValidToken_WhenVerifyingEmail_ThenEmailVerified()
    {
        var context = new IdentityUseCaseTestContext();
        var registered = context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            DateTimeOffset.UtcNow);

        context.VerifyEmail.Handle(
            new VerifyEmailCommand("verification-token-1"),
            DateTimeOffset.UtcNow);

        var user = context.LoadUser(registered.UserId);
        var userView = context.FindUser(registered.UserId);

        Assert.NotNull(user.EmailVerifiedAt);
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationTokenExpiresAt);
        Assert.NotNull(userView);
        Assert.NotNull(userView.EmailVerifiedAt);
        Assert.Null(userView.EmailVerificationToken);
    }

    [Fact]
    public void GivenInvalidToken_WhenVerifyingEmail_ThenEmailVerificationMustBeValidViolation()
    {
        var context = new IdentityUseCaseTestContext();

        context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<SemanticViolationException>(() =>
            context.VerifyEmail.Handle(
                new VerifyEmailCommand("not-the-token"),
                DateTimeOffset.UtcNow));

        Assert.Equal("Email verification must be valid.", exception.Message);
    }

    [Fact]
    public void GivenExpiredToken_WhenVerifyingEmail_ThenEmailVerificationMustBeValidViolation()
    {
        var context = new IdentityUseCaseTestContext();

        context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<SemanticViolationException>(() =>
            context.VerifyEmail.Handle(
                new VerifyEmailCommand("verification-token-1"),
                DateTimeOffset.UtcNow.AddDays(2)));

        Assert.Equal("Email verification must be valid.", exception.Message);
    }
}
