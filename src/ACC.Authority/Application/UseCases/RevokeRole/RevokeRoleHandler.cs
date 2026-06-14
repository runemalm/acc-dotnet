using ACC.Authority.Application.Policies;
using ACC.Authority.Application.Ports.Identity;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Events;
using ACC.Authority.Domain.Invariants;
using ACC.Authority.Domain.Powers;
using ACC.Authority.Infrastructure.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.EventSourcing;

namespace ACC.Authority.Application.UseCases.RevokeRole;

public sealed class RevokeRoleHandler
{
    private readonly EventSourcedRepository<RoleAssignment> roleAssignments;
    private readonly RoleAssignmentProjection roleAssignmentProjection;
    private readonly IRecognizedUserPort recognizedUsers;
    private readonly IAuthorityPolicy authorityPolicy;

    public RevokeRoleHandler(
        EventSourcedRepository<RoleAssignment> roleAssignments,
        RoleAssignmentProjection roleAssignmentProjection,
        IRecognizedUserPort recognizedUsers,
        IAuthorityPolicy authorityPolicy)
    {
        this.roleAssignments = roleAssignments;
        this.roleAssignmentProjection = roleAssignmentProjection;
        this.recognizedUsers = recognizedUsers;
        this.authorityPolicy = authorityPolicy;
    }

    public RevokeRoleResult Handle(RevokeRoleCommand command, DateTimeOffset revokedAt)
    {
        ArgumentNullException.ThrowIfNull(command);

        UserMustBeRecognizedForAuthority.Ensure(
            recognizedUsers.IsRecognizedUser(command.ActorUserId),
            command.ActorUserId);
        var roleAssignment = roleAssignments.Load(RoleAssignmentStream(command.RoleAssignmentId));
        if (roleAssignment.Id == Guid.Empty)
        {
            throw new InvalidOperationException("A role assignment must exist before it can be revoked.");
        }

        ActorMustHavePower.Ensure(
            authorityPolicy.HasPower(
                command.ActorUserId,
                roleAssignment.AccountingSubjectId,
                Power.RevokeRole),
            command.ActorUserId,
            roleAssignment.AccountingSubjectId,
            Power.RevokeRole);

        roleAssignment.Revoke(command.ActorUserId, revokedAt);

        var storedEvents = roleAssignments.Save(RoleAssignmentStream(command.RoleAssignmentId), roleAssignment);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<RoleRevoked>()
            .Single();

        roleAssignmentProjection.Project(domainEvent);

        return new RevokeRoleResult();
    }

    private static StreamId RoleAssignmentStream(Guid roleAssignmentId) =>
        StreamId.For("role-assignment", roleAssignmentId);
}
