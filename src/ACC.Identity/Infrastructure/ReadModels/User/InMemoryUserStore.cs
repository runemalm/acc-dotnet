using System.Collections.Concurrent;
using ACC.Identity.Application.Ports.ReadModels.User;

namespace ACC.Identity.Infrastructure.ReadModels.User;

public sealed class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<Guid, UserView> users = new();

    public UserView? Find(Guid userId) =>
        users.GetValueOrDefault(userId);

    public UserView? FindByEmail(string normalizedEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);

        return users.Values.SingleOrDefault(user =>
            user.NormalizedEmail == normalizedEmail);
    }

    public UserView? FindByEmailVerificationToken(string emailVerificationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailVerificationToken);

        return users.Values.SingleOrDefault(user =>
            user.EmailVerificationToken == emailVerificationToken);
    }

    public void Save(UserView user)
    {
        ArgumentNullException.ThrowIfNull(user);

        users[user.UserId] = user;
    }
}
