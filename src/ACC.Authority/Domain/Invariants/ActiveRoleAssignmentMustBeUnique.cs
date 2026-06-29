using ACC.Authority.Domain.Aggregates;
using ACC.BuildingBlocks.Domain;

namespace ACC.Authority.Domain.Invariants;

public static class ActiveRoleAssignmentMustBeUnique
{
    public static void Ensure(
        bool isUnique,
        Guid userId,
        Guid accountingSubjectId,
        Role role)
    {
        if (!isUnique)
        {
            throw new ActiveRoleAssignmentMustBeUniqueViolation(
                userId,
                accountingSubjectId,
                role);
        }
    }
}

public sealed class ActiveRoleAssignmentMustBeUniqueViolation(
    Guid userId,
    Guid accountingSubjectId,
    Role role)
    : InvariantViolationException(
        $"User {userId} already has active role {role} for accounting subject {accountingSubjectId}.");
