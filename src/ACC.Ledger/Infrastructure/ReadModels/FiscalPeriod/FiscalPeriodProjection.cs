using ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Domain.Events;

namespace ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;

public sealed class FiscalPeriodProjection
{
    private readonly IFiscalPeriodStore fiscalPeriods;

    public FiscalPeriodProjection(IFiscalPeriodStore fiscalPeriods)
    {
        this.fiscalPeriods = fiscalPeriods;
    }

    public void Project(FiscalPeriodOpened domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        fiscalPeriods.Save(new FiscalPeriodView(
            domainEvent.FiscalPeriodId,
            domainEvent.AccountingSubjectId,
            domainEvent.StartsOn,
            domainEvent.EndsOn,
            FiscalPeriodStatus.Open));
    }

    public void Project(FiscalPeriodClosed domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var fiscalPeriod = fiscalPeriods.Find(domainEvent.FiscalPeriodId)
            ?? throw new InvalidOperationException(
                $"Fiscal period {domainEvent.FiscalPeriodId} could not be found.");

        fiscalPeriods.Save(fiscalPeriod with { Status = FiscalPeriodStatus.Closed });
    }
}
