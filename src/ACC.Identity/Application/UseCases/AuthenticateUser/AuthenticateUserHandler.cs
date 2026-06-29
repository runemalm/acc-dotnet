using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
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
        ValidateCommand(command);

        var existingUser = userStore.FindByEmail(NormalizeEmail(command.Email));

        if (existingUser is null)
        {
            throw new AuthenticationFailedException("Authentication must be valid.");
        }

        var user = users.Load(UserStream(existingUser.UserId));

        UserMustBeActiveToAuthenticate.Ensure(user);
        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new AuthenticationFailedException("Authentication must be valid.");
        }

        var token = tokenIssuer.Issue(user.Id, user.Email, authenticatedAt);

        return new AuthenticateUserResult(
            token.AccessToken,
            token.ExpiresAt);
    }

    private static StreamId UserStream(Guid userId) =>
        StreamId.For("user", userId);

    private static void ValidateCommand(AuthenticateUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ApplicationValidationException(
                "Authentication must identify an email address and password.");
        }
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToUpperInvariant();
}
