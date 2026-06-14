using ACC.Authority.Domain.Aggregates;

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
            throw new InvalidOperationException(
                $"User {userId} already has active role {role} for accounting subject {accountingSubjectId}.");
        }
    }
}
