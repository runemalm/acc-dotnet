using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Infrastructure.ReadModels.RoleAssignment;

public sealed class PostgresRoleAssignmentStore : IRoleAssignmentStore
{
    public RoleAssignmentView? Find(Guid roleAssignmentId) =>
        throw new NotSupportedException("Postgres role assignment read model persistence has not been implemented yet.");

    public IReadOnlyCollection<RoleAssignmentView> FindActiveByUserId(Guid userId) =>
        throw new NotSupportedException("Postgres role assignment read model persistence has not been implemented yet.");

    public bool ActiveAssignmentExists(
        Guid userId,
        Guid accountingSubjectId,
        Role role) =>
        throw new NotSupportedException("Postgres role assignment read model persistence has not been implemented yet.");

    public void Save(RoleAssignmentView roleAssignment) =>
        throw new NotSupportedException("Postgres role assignment read model persistence has not been implemented yet.");
}
