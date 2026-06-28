using ACC.Authority.Domain.Events;
using ACC.Authority.Domain.Invariants;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;

namespace ACC.Authority.Domain.Aggregates;

public sealed class RoleAssignment : EventSourcedAggregate
{
    private RoleAssignment()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid AccountingSubjectId { get; private set; }

    public Role Role { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null;

    public void Revoke(Guid revokedByUserId, DateTimeOffset revokedAt)
    {
        if (Id == Guid.Empty)
        {
            throw new ResourceNotFoundException("A role assignment must exist before it can be revoked.");
        }

        if (revokedByUserId == Guid.Empty)
        {
            throw new ArgumentException("A role revocation must identify the acting user.", nameof(revokedByUserId));
        }

        RoleAssignmentMustBeActiveToRevoke.Ensure(this);

        Raise(new RoleRevoked(
            Id,
            revokedByUserId,
            UserId,
            AccountingSubjectId,
            Role,
            revokedAt));
    }

    public static RoleAssignment Assign(
        Guid id,
        Guid assignedByUserId,
        Guid userId,
        Guid accountingSubjectId,
        Role role,
        DateTimeOffset assignedAt)
    {
        EnsureCanAssign(id, assignedByUserId, userId, accountingSubjectId, role);

        var roleAssignment = new RoleAssignment();
        roleAssignment.Raise(new RoleAssigned(
            id,
            assignedByUserId,
            userId,
            accountingSubjectId,
            role,
            assignedAt));

        return roleAssignment;
    }

    public static RoleAssignment EstablishInitialOwner(
        Guid id,
        Guid actorUserId,
        Guid accountingSubjectId,
        DateTimeOffset assignedAt) =>
        Assign(
            id,
            actorUserId,
            actorUserId,
            accountingSubjectId,
            Role.Owner,
            assignedAt);

    public static RoleAssignment Rehydrate(IEnumerable<object> events)
    {
        var roleAssignment = new RoleAssignment();
        roleAssignment.LoadFromHistory(events);

        return roleAssignment;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RoleAssigned assigned:
                Apply(assigned);
                break;
            case RoleRevoked revoked:
                Apply(revoked);
                break;
        }
    }

    private static void EnsureCanAssign(
        Guid id,
        Guid assignedByUserId,
        Guid userId,
        Guid accountingSubjectId,
        Role role)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A role assignment must have an identity.", nameof(id));
        }

        if (assignedByUserId == Guid.Empty)
        {
            throw new ArgumentException("A role assignment must identify the acting user.", nameof(assignedByUserId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A role assignment must identify the user receiving authority.", nameof(userId));
        }

        if (accountingSubjectId == Guid.Empty)
        {
            throw new ArgumentException(
                "A role assignment must identify the accounting subject.",
                nameof(accountingSubjectId));
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }
    }

    private void Apply(RoleAssigned domainEvent)
    {
        Id = domainEvent.RoleAssignmentId;
        UserId = domainEvent.UserId;
        AccountingSubjectId = domainEvent.AccountingSubjectId;
        Role = domainEvent.Role;
        AssignedAt = domainEvent.AssignedAt;
        RevokedAt = null;
    }

    private void Apply(RoleRevoked domainEvent)
    {
        RevokedAt = domainEvent.RevokedAt;
    }
}
