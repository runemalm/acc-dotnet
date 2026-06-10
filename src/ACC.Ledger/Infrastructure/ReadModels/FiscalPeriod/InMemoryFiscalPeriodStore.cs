using System.Collections.Concurrent;
using ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;

namespace ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;

public sealed class InMemoryFiscalPeriodStore : IFiscalPeriodStore
{
    private readonly ConcurrentDictionary<Guid, FiscalPeriodView> fiscalPeriods = new();

    public FiscalPeriodView? Find(Guid fiscalPeriodId) =>
        fiscalPeriods.GetValueOrDefault(fiscalPeriodId);

    public FiscalPeriodView? FindFor(Guid accountingSubjectId, DateOnly accountingDate) =>
        fiscalPeriods.Values.SingleOrDefault(fiscalPeriod =>
            fiscalPeriod.AccountingSubjectId == accountingSubjectId &&
            fiscalPeriod.StartsOn <= accountingDate &&
            accountingDate <= fiscalPeriod.EndsOn);

    public void Save(FiscalPeriodView fiscalPeriod)
    {
        ArgumentNullException.ThrowIfNull(fiscalPeriod);

        fiscalPeriods[fiscalPeriod.FiscalPeriodId] = fiscalPeriod;
    }
}
