namespace ACC.Ledger.Application.UseCases.OpenFiscalPeriod;

public sealed record OpenFiscalPeriodCommand(
    Guid AccountingSubjectId,
    DateOnly StartsOn,
    DateOnly EndsOn);
