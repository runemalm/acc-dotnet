namespace ACC.Authority.Domain.Aggregates;

public sealed class RoleAssignment
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public Guid AccountingSubjectId { get; init; }

    public Role Role { get; init; }

    public DateTimeOffset AssignedAt { get; init; }

    public DateTimeOffset? RevokedAt { get; init; }
}
