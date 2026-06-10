using ACC.Identity.Application.Ports.ReadModels.User;
using ACC.Identity.Domain.Events;

namespace ACC.Identity.Infrastructure.ReadModels.User;

public sealed class UserProjection
{
    private readonly IUserStore users;

    public UserProjection(IUserStore users)
    {
        this.users = users;
    }

    public void Project(UserRegistered domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        users.Save(new UserView(
            domainEvent.UserId,
            domainEvent.Email,
            NormalizeEmail(domainEvent.Email),
            IsActive: true,
            domainEvent.EmailVerificationToken,
            EmailVerifiedAt: null));
    }

    public void Project(EmailVerified domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var user = users.Find(domainEvent.UserId);

        if (user is null)
        {
            return;
        }

        users.Save(user with
        {
            EmailVerificationToken = null,
            EmailVerifiedAt = domainEvent.VerifiedAt
        });
    }

    public void Project(EmailVerificationResent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var user = users.Find(domainEvent.UserId);

        if (user is null)
        {
            return;
        }

        users.Save(user with
        {
            EmailVerificationToken = domainEvent.EmailVerificationToken
        });
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToUpperInvariant();
}
