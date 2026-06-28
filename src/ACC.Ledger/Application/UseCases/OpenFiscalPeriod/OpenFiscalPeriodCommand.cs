namespace ACC.Ledger.Application.UseCases.OpenFiscalPeriod;

public sealed record OpenFiscalPeriodCommand(
    Guid ActorUserId,
    Guid AccountingSubjectId,
    DateOnly StartsOn,
    DateOnly EndsOn);
