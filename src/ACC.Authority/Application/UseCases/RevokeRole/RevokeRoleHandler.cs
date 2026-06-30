using ACC.Authority.Application.Policies;
using ACC.Authority.Application.Ports.Identity;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Events;
using ACC.Authority.Domain.Invariants;
using ACC.Authority.Domain.Powers;
using ACC.Authority.Infrastructure.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;

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
        ValidateCommand(command);

        if (!recognizedUsers.IsRecognizedUser(command.ActorUserId))
        {
            throw new RequiredObjectNotFoundException(
                $"Acting user {command.ActorUserId} is required to revoke a role.");
        }

        var roleAssignment = roleAssignments.Load(RoleAssignmentStream(command.RoleAssignmentId));
        if (roleAssignment.Id == Guid.Empty)
        {
            throw new RequiredObjectNotFoundException(
                "A role assignment must exist before it can be revoked.");
        }

        if (!authorityPolicy.HasPower(
                command.ActorUserId,
                roleAssignment.AccountingSubjectId,
                Power.RevokeRole))
        {
            throw new AuthorizationDeniedException(
                $"User {command.ActorUserId} must have RevokeRole power for accounting subject {roleAssignment.AccountingSubjectId}.");
        }

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

    private static void ValidateCommand(RevokeRoleCommand command)
    {
        if (command.ActorUserId == Guid.Empty || command.RoleAssignmentId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Revoking a role must identify the acting user and role assignment.");
        }
    }
}
