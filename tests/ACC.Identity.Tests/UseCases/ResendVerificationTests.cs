using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.ResendVerification;
using ACC.Identity.Application.UseCases.VerifyEmail;
using ACC.Identity.Tests.TestKit;
using Xunit;

namespace ACC.Identity.Tests.UseCases;

public sealed class ResendVerificationTests
{
    [Fact]
    public void GivenUnverifiedUser_WhenResendingVerification_ThenEmailVerificationResent()
    {
        var context = new IdentityUseCaseTestContext();
        var registered = context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            DateTimeOffset.UtcNow);

        var beforeResend = context.LoadUser(registered.UserId);

        context.ResendVerification.Handle(
            new ResendVerificationCommand("USER@example.com"),
            DateTimeOffset.UtcNow);

        var afterResend = context.LoadUser(registered.UserId);

        Assert.NotEqual(
            beforeResend.EmailVerificationToken,
            afterResend.EmailVerificationToken);
        Assert.Equal("verification-token-2", afterResend.EmailVerificationToken);
        Assert.Collection(
            context.EmailSender.SentVerificationEmails,
            registrationEmail => Assert.Equal("verification-token-1", registrationEmail.VerificationToken),
            resentEmail => Assert.Equal("verification-token-2", resentEmail.VerificationToken));

        context.VerifyEmail.Handle(
            new VerifyEmailCommand("verification-token-2"),
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public void GivenUnknownEmail_WhenResendingVerification_ThenRequestIsIndistinguishable()
    {
        var context = new IdentityUseCaseTestContext();

        context.ResendVerification.Handle(
            new ResendVerificationCommand("missing@example.com"),
            DateTimeOffset.UtcNow);

        Assert.Empty(context.EmailSender.SentVerificationEmails);
    }

    [Fact]
    public void GivenVerifiedEmail_WhenResendingVerification_ThenRequestIsIndistinguishable()
    {
        var context = new IdentityUseCaseTestContext();
        context.RegisterUser.Handle(
            new RegisterUserCommand("user@example.com", "correct horse battery staple"),
            DateTimeOffset.UtcNow);
        context.VerifyEmail.Handle(
            new VerifyEmailCommand("verification-token-1"),
            DateTimeOffset.UtcNow);

        context.ResendVerification.Handle(
            new ResendVerificationCommand("user@example.com"),
            DateTimeOffset.UtcNow);

        Assert.Single(context.EmailSender.SentVerificationEmails);
    }
}
