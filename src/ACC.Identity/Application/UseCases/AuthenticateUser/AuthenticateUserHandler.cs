using ACC.BuildingBlocks.EventSourcing;
using ACC.Identity.Application.Ports.ReadModels.User;
using ACC.Identity.Application.Ports.Security;
using ACC.Identity.Domain.Aggregates;
using ACC.Identity.Domain.Invariants;

namespace ACC.Identity.Application.UseCases.AuthenticateUser;

public sealed class AuthenticateUserHandler
{
    private readonly EventSourcedRepository<User> users;
    private readonly IUserStore userStore;
    private readonly IPasswordHasher passwordHasher;
    private readonly IAuthenticationTokenIssuer tokenIssuer;

    public AuthenticateUserHandler(
        EventSourcedRepository<User> users,
        IUserStore userStore,
        IPasswordHasher passwordHasher,
        IAuthenticationTokenIssuer tokenIssuer)
    {
        this.users = users;
        this.userStore = userStore;
        this.passwordHasher = passwordHasher;
        this.tokenIssuer = tokenIssuer;
    }

    public AuthenticateUserResult Handle(AuthenticateUserCommand command, DateTimeOffset authenticatedAt)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existingUser = userStore.FindByEmail(NormalizeEmail(command.Email));

        AuthenticationMustBeValid.Ensure(existingUser is not null);

        var user = users.Load(UserStream(existingUser!.UserId));

        UserMustBeActiveToAuthenticate.Ensure(user);
        AuthenticationMustBeValid.Ensure(
            passwordHasher.Verify(command.Password, user.PasswordHash));

        var token = tokenIssuer.Issue(user.Id, user.Email, authenticatedAt);

        return new AuthenticateUserResult(
            token.AccessToken,
            token.ExpiresAt);
    }

    private static StreamId UserStream(Guid userId) =>
        StreamId.For("user", userId);

    private static string NormalizeEmail(string email) =>
        email.Trim().ToUpperInvariant();
}
