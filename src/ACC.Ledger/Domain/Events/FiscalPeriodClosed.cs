namespace ACC.Ledger.Domain.Events;

public sealed record FiscalPeriodClosed(
    Guid FiscalPeriodId,
    Guid AccountingSubjectId,
    DateTimeOffset OccurredAt);
