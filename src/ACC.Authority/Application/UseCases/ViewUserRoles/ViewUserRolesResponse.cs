namespace ACC.Authority.Application.UseCases.ViewUserRoles;

public sealed record ViewUserRolesResponse(
    Guid UserId,
    IReadOnlyCollection<ViewUserRoleResponse> Roles);

public sealed record ViewUserRoleResponse(
    Guid RoleAssignmentId,
    Guid AccountingSubjectId,
    string Role,
    DateTimeOffset AssignedAt);
