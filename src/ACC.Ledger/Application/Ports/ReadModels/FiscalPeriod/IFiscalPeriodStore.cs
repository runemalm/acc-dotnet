namespace ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;

public interface IFiscalPeriodStore
{
    FiscalPeriodView? Find(Guid fiscalPeriodId);

    FiscalPeriodView? FindFor(Guid accountingSubjectId, DateOnly accountingDate);

    void Save(FiscalPeriodView fiscalPeriod);
}
