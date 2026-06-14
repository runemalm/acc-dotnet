using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Application.Ports.ReadModels.RoleAssignment;

public sealed record RoleAssignmentView(
    Guid RoleAssignmentId,
    Guid UserId,
    Guid AccountingSubjectId,
    Role Role,
    DateTimeOffset AssignedAt,
    DateTimeOffset? RevokedAt)
{
    public bool IsActive => RevokedAt is null;
}
