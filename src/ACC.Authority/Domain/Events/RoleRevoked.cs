using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Domain.Events;

public sealed record RoleRevoked(
    Guid RoleAssignmentId,
    Guid RevokedByUserId,
    Guid UserId,
    Guid AccountingSubjectId,
    Role Role,
    DateTimeOffset RevokedAt);
