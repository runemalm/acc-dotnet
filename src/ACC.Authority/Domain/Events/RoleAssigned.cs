using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Domain.Events;

public sealed record RoleAssigned(
    Guid RoleAssignmentId,
    Guid AssignedByUserId,
    Guid UserId,
    Guid AccountingSubjectId,
    Role Role,
    DateTimeOffset AssignedAt);
