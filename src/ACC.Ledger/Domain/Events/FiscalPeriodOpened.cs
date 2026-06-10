namespace ACC.Ledger.Domain.Events;

public sealed record FiscalPeriodOpened(
    Guid FiscalPeriodId,
    Guid AccountingSubjectId,
    DateOnly StartsOn,
    DateOnly EndsOn,
    DateTimeOffset OccurredAt);
