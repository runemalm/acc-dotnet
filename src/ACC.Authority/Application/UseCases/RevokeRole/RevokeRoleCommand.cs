namespace ACC.Authority.Application.UseCases.RevokeRole;

public sealed record RevokeRoleCommand(
    Guid ActorUserId,
    Guid RoleAssignmentId);
