using ACC.Ledger.Domain.Aggregates;

namespace ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;

public sealed record FiscalPeriodView(
    Guid FiscalPeriodId,
    Guid AccountingSubjectId,
    DateOnly StartsOn,
    DateOnly EndsOn,
    FiscalPeriodStatus Status);
