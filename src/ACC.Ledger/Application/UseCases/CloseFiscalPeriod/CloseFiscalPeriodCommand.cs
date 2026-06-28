namespace ACC.Ledger.Application.UseCases.CloseFiscalPeriod;

public sealed record CloseFiscalPeriodCommand(
    Guid ActorUserId,
    Guid FiscalPeriodId);
