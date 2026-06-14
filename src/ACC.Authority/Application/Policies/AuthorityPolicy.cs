using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Domain.Powers;

namespace ACC.Authority.Application.Policies;

public sealed class AuthorityPolicy : IAuthorityPolicy
{
    private readonly IRoleAssignmentStore roleAssignments;
    private readonly RolePowerPolicy rolePowerPolicy;

    public AuthorityPolicy(
        IRoleAssignmentStore roleAssignments,
        RolePowerPolicy rolePowerPolicy)
    {
        this.roleAssignments = roleAssignments;
        this.rolePowerPolicy = rolePowerPolicy;
    }

    public bool HasPower(
        Guid actorUserId,
        Guid accountingSubjectId,
        Power power) =>
        roleAssignments.FindActiveByUserId(actorUserId)
            .Where(roleAssignment => roleAssignment.AccountingSubjectId == accountingSubjectId)
            .Any(roleAssignment => rolePowerPolicy.Grants(roleAssignment.Role, power));
}
