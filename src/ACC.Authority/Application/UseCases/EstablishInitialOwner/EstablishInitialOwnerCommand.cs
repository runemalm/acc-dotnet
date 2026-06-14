namespace ACC.Authority.Application.UseCases.EstablishInitialOwner;

public sealed record EstablishInitialOwnerCommand(
    Guid ActorUserId,
    Guid AccountingSubjectId);
