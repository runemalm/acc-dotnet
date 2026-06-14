using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Application.UseCases.AssignRole;

public sealed record AssignRoleCommand(
    Guid ActorUserId,
    Guid UserId,
    Guid AccountingSubjectId,
    Role Role);
