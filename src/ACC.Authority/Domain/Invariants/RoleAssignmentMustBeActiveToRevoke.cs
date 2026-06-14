using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Domain.Invariants;

public static class RoleAssignmentMustBeActiveToRevoke
{
    public static void Ensure(RoleAssignment roleAssignment)
    {
        ArgumentNullException.ThrowIfNull(roleAssignment);

        if (!roleAssignment.IsActive)
        {
            throw new InvalidOperationException("A role assignment must be active before it can be revoked.");
        }
    }
}
