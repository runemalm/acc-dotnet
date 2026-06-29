using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.Failures;

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
        ValidateQuery(query);

        var roles = roleAssignments.FindActiveByUserId(query.ActorUserId)
            .Select(roleAssignment => new ViewUserRoleResponse(
                roleAssignment.RoleAssignmentId,
                roleAssignment.AccountingSubjectId,
                roleAssignment.Role.ToString(),
                roleAssignment.AssignedAt))
            .ToArray();

        return new ViewUserRolesResponse(query.ActorUserId, roles);
    }

    private static void ValidateQuery(ViewUserRolesQuery query)
    {
        if (query.ActorUserId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Viewing user roles must identify the acting user.");
        }
    }
}
