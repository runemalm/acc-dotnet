using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;

namespace ACC.Authority.Application.UseCases.ViewUserRoles;

public sealed class ViewUserRolesHandler
{
    private readonly IRoleAssignmentStore roleAssignments;

    public ViewUserRolesHandler(IRoleAssignmentStore roleAssignments)
    {
        this.roleAssignments = roleAssignments;
    }

    public ViewUserRolesResponse Handle(ViewUserRolesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var roles = roleAssignments.FindActiveByUserId(query.UserId)
            .Select(roleAssignment => new ViewUserRoleResponse(
                roleAssignment.RoleAssignmentId,
                roleAssignment.AccountingSubjectId,
                roleAssignment.Role.ToString(),
                roleAssignment.AssignedAt))
            .ToArray();

        return new ViewUserRolesResponse(query.UserId, roles);
    }
}
