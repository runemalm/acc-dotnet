using ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;

namespace ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;

internal sealed class PostgresFiscalPeriodStore : IFiscalPeriodStore
{
    public FiscalPeriodView? Find(Guid fiscalPeriodId) =>
        throw new NotImplementedException("Postgres fiscal period read model has not been implemented yet.");

    public FiscalPeriodView? FindFor(Guid accountingSubjectId, DateOnly accountingDate) =>
        throw new NotImplementedException("Postgres fiscal period read model has not been implemented yet.");

    public void Save(FiscalPeriodView fiscalPeriod) =>
        throw new NotImplementedException("Postgres fiscal period read model has not been implemented yet.");
}
