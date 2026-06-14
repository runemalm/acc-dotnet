using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Domain.Events;

namespace ACC.Authority.Infrastructure.ReadModels.RoleAssignment;

public sealed class RoleAssignmentProjection
{
    private readonly IRoleAssignmentStore roleAssignments;

    public RoleAssignmentProjection(IRoleAssignmentStore roleAssignments)
    {
        this.roleAssignments = roleAssignments;
    }

    public void Project(RoleAssigned domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        roleAssignments.Save(new RoleAssignmentView(
            domainEvent.RoleAssignmentId,
            domainEvent.UserId,
            domainEvent.AccountingSubjectId,
            domainEvent.Role,
            domainEvent.AssignedAt,
            RevokedAt: null));
    }

    public void Project(RoleRevoked domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var roleAssignment = roleAssignments.Find(domainEvent.RoleAssignmentId)
            ?? throw new InvalidOperationException(
                $"Role assignment {domainEvent.RoleAssignmentId} could not be found.");

        roleAssignments.Save(roleAssignment with { RevokedAt = domainEvent.RevokedAt });
    }
}
