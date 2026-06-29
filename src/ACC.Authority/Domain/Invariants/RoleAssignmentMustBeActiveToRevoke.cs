using ACC.Authority.Domain.Aggregates;
using ACC.BuildingBlocks.Domain;

namespace ACC.Authority.Domain.Invariants;

public static class RoleAssignmentMustBeActiveToRevoke
{
    public static void Ensure(RoleAssignment roleAssignment)
    {
        ArgumentNullException.ThrowIfNull(roleAssignment);

        if (!roleAssignment.IsActive)
        {
            throw new RoleAssignmentMustBeActiveToRevokeViolation();
        }
    }
}

public sealed class RoleAssignmentMustBeActiveToRevokeViolation()
    : InvariantViolationException("A role assignment must be active before it can be revoked.");
