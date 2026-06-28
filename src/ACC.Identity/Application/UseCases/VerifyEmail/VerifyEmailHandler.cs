using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.Identity.Application.Ports.ReadModels.User;
using ACC.Identity.Domain.Aggregates;
using ACC.Identity.Domain.Events;
using ACC.Identity.Infrastructure.ReadModels.User;

namespace ACC.Identity.Application.UseCases.VerifyEmail;

public sealed class VerifyEmailHandler
{
    private readonly EventSourcedRepository<User> users;
    private readonly IUserStore userStore;
    private readonly UserProjection userProjection;

    public VerifyEmailHandler(
        EventSourcedRepository<User> users,
        IUserStore userStore,
        UserProjection userProjection)
    {
        this.users = users;
        this.userStore = userStore;
        this.userProjection = userProjection;
    }

    public VerifyEmailResult Handle(VerifyEmailCommand command, DateTimeOffset verifiedAt)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Token))
        {
            throw new SemanticViolationException("Email verification must be valid.");
        }

        var existingUser = userStore.FindByEmailVerificationToken(command.Token);

        if (existingUser is null)
        {
            throw new SemanticViolationException("Email verification must be valid.");
        }

        var streamId = UserStream(existingUser.UserId);
        var user = users.Load(streamId);

        user.VerifyEmail(command.Token, verifiedAt);

        var storedEvents = users.Save(streamId, user);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<EmailVerified>()
            .Single();

        userProjection.Project(domainEvent);

        return new VerifyEmailResult();
    }

    private static StreamId UserStream(Guid userId) =>
        StreamId.For("user", userId);
}
