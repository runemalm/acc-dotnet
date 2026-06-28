using ACC.BuildingBlocks.EventSourcing;
using ACC.Identity.Application.Ports.Communication;
using ACC.Identity.Application.Ports.ReadModels.User;
using ACC.Identity.Application.Ports.Security;
using ACC.Identity.Domain.Aggregates;
using ACC.Identity.Domain.Invariants;
using ACC.Identity.Infrastructure.ReadModels.User;

namespace ACC.Identity.Application.UseCases.ResendVerification;

public sealed class ResendVerificationHandler
{
    private readonly EventSourcedRepository<User> users;
    private readonly IUserStore userStore;
    private readonly IEmailVerificationTokenGenerator tokenGenerator;
    private readonly IIdentityEmailSender emailSender;
    private readonly UserProjection userProjection;

    public ResendVerificationHandler(
        EventSourcedRepository<User> users,
        IUserStore userStore,
        IEmailVerificationTokenGenerator tokenGenerator,
        IIdentityEmailSender emailSender,
        UserProjection userProjection)
    {
        this.users = users;
        this.userStore = userStore;
        this.tokenGenerator = tokenGenerator;
        this.emailSender = emailSender;
        this.userProjection = userProjection;
    }

    public ResendVerificationResult Handle(ResendVerificationCommand command, DateTimeOffset resentAt)
    {
        ArgumentNullException.ThrowIfNull(command);

        UserEmailMustBeValid.Ensure(command.Email);
        var email = command.Email.Trim();
        var existingUser = userStore.FindByEmail(NormalizeEmail(email));

        if (existingUser is null || existingUser.EmailVerifiedAt is not null)
        {
            return new ResendVerificationResult();
        }

        var streamId = UserStream(existingUser.UserId);
        var user = users.Load(streamId);

        user.ResendVerification(
            tokenGenerator.Generate(),
            resentAt.AddHours(24),
            resentAt);

        var storedEvents = users.Save(streamId, user);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<Domain.Events.EmailVerificationResent>()
            .Single();

        userProjection.Project(domainEvent);

        emailSender.SendVerificationEmail(
            user.Email,
            domainEvent.EmailVerificationToken,
            domainEvent.EmailVerificationTokenExpiresAt);

        return new ResendVerificationResult();
    }

    private static StreamId UserStream(Guid userId) =>
        StreamId.For("user", userId);

    private static string NormalizeEmail(string email) =>
        email.Trim().ToUpperInvariant();
}
