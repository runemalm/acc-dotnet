using ACC.BuildingBlocks.EventSourcing;
using ACC.Identity.Domain.Events;
using ACC.Identity.Domain.Invariants;

namespace ACC.Identity.Domain.Aggregates;

public sealed class User : EventSourcedAggregate
{
    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public DateTimeOffset? EmailVerifiedAt { get; private set; }

    public string? EmailVerificationToken { get; private set; }

    public DateTimeOffset? EmailVerificationTokenExpiresAt { get; private set; }

    public void VerifyEmail(string token, DateTimeOffset verifiedAt)
    {
        EmailMustNotAlreadyBeVerified.Ensure(this);
        EmailVerificationMustBeValid.Ensure(this, token, verifiedAt);

        Raise(new EmailVerified(Id, verifiedAt));
    }

    public void ResendVerification(
        string emailVerificationToken,
        DateTimeOffset emailVerificationTokenExpiresAt,
        DateTimeOffset resentAt)
    {
        EmailMustNotAlreadyBeVerified.Ensure(this);

        if (string.IsNullOrWhiteSpace(emailVerificationToken))
        {
            throw new ArgumentException(
                "Resending email verification must issue an email verification token.",
                nameof(emailVerificationToken));
        }

        Raise(new EmailVerificationResent(
            Id,
            emailVerificationToken,
            emailVerificationTokenExpiresAt,
            resentAt));
    }

    public static User Register(
        Guid id,
        string email,
        string passwordHash,
        string emailVerificationToken,
        DateTimeOffset emailVerificationTokenExpiresAt,
        DateTimeOffset registeredAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A user must have an identity.", nameof(id));
        }

        UserEmailMustBeValid.Ensure(email);

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("A user must have a password hash.", nameof(passwordHash));
        }

        if (string.IsNullOrWhiteSpace(emailVerificationToken))
        {
            throw new ArgumentException(
                "A user registration must issue an email verification token.",
                nameof(emailVerificationToken));
        }

        var user = new User();
        user.Raise(new UserRegistered(
            id,
            email,
            passwordHash,
            emailVerificationToken,
            emailVerificationTokenExpiresAt,
            registeredAt));

        return user;
    }

    public static User Rehydrate(IEnumerable<object> events)
    {
        var user = new User();
        user.LoadFromHistory(events);

        return user;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case UserRegistered registered:
                Apply(registered);
                break;
            case EmailVerificationResent resent:
                Apply(resent);
                break;
            case EmailVerified verified:
                Apply(verified);
                break;
        }
    }

    private void Apply(UserRegistered domainEvent)
    {
        Id = domainEvent.UserId;
        Email = domainEvent.Email;
        IsActive = true;
        PasswordHash = domainEvent.PasswordHash;
        EmailVerifiedAt = null;
        EmailVerificationToken = domainEvent.EmailVerificationToken;
        EmailVerificationTokenExpiresAt = domainEvent.EmailVerificationTokenExpiresAt;
    }

    private void Apply(EmailVerificationResent domainEvent)
    {
        EmailVerificationToken = domainEvent.EmailVerificationToken;
        EmailVerificationTokenExpiresAt = domainEvent.EmailVerificationTokenExpiresAt;
    }

    private void Apply(EmailVerified domainEvent)
    {
        EmailVerifiedAt = domainEvent.VerifiedAt;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
    }
}
