using ACC.BuildingBlocks.EventSourcing;
using ACC.Identity.Application.Ports.Communication;
using ACC.Identity.Application.Ports.ReadModels.User;
using ACC.Identity.Application.Ports.Security;
using ACC.Identity.Domain.Aggregates;
using ACC.Identity.Domain.Events;
using ACC.Identity.Domain.Invariants;
using ACC.Identity.Infrastructure.ReadModels.User;

namespace ACC.Identity.Application.UseCases.RegisterUser;

public sealed class RegisterUserHandler
{
    private readonly EventSourcedRepository<User> users;
    private readonly IUserStore userStore;
    private readonly UserProjection userProjection;
    private readonly IPasswordHasher passwordHasher;
    private readonly IEmailVerificationTokenGenerator tokenGenerator;
    private readonly IIdentityEmailSender emailSender;
    private readonly IAuthenticationTokenIssuer tokenIssuer;

    public RegisterUserHandler(
        EventSourcedRepository<User> users,
        IUserStore userStore,
        UserProjection userProjection,
        IPasswordHasher passwordHasher,
        IEmailVerificationTokenGenerator tokenGenerator,
        IIdentityEmailSender emailSender,
        IAuthenticationTokenIssuer tokenIssuer)
    {
        this.users = users;
        this.userStore = userStore;
        this.userProjection = userProjection;
        this.passwordHasher = passwordHasher;
        this.tokenGenerator = tokenGenerator;
        this.emailSender = emailSender;
        this.tokenIssuer = tokenIssuer;
    }

    public RegisterUserResult Handle(RegisterUserCommand command, DateTimeOffset registeredAt)
    {
        ArgumentNullException.ThrowIfNull(command);

        var email = command.Email;
        UserEmailMustBeValid.Ensure(email);
        var normalizedEmail = NormalizeEmail(email);

        UserEmailMustBeUnique.Ensure(
            userStore.FindByEmail(normalizedEmail) is null,
            email);

        var userId = Guid.NewGuid();
        var user = User.Register(
            userId,
            email,
            passwordHasher.Hash(command.Password),
            tokenGenerator.Generate(),
            registeredAt.AddHours(24),
            registeredAt);

        var storedEvents = users.Save(UserStream(userId), user);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<UserRegistered>()
            .Single();

        userProjection.Project(domainEvent);
        emailSender.SendVerificationEmail(
            domainEvent.Email,
            domainEvent.EmailVerificationToken,
            domainEvent.EmailVerificationTokenExpiresAt);
        var authenticationToken = tokenIssuer.Issue(
            user.Id,
            user.Email,
            registeredAt);

        return new RegisterUserResult(
            user.Id,
            authenticationToken.AccessToken,
            authenticationToken.ExpiresAt);
    }

    private static StreamId UserStream(Guid userId) =>
        StreamId.For("user", userId);

    private static string NormalizeEmail(string email) =>
        email.Trim().ToUpperInvariant();
}
