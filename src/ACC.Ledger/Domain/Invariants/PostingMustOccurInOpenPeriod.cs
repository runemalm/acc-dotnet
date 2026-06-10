using ACC.Ledger.Domain.Aggregates;

namespace ACC.Ledger.Domain.Invariants;

public static class PostingMustOccurInOpenPeriod
{
    public static void Ensure(DateOnly accountingDate, FiscalPeriod? fiscalPeriod)
    {
        if (fiscalPeriod is null)
        {
            throw new InvalidOperationException(
                $"Journal entry cannot be posted on {accountingDate} because no fiscal period contains that date.");
        }

        if (!fiscalPeriod.Contains(accountingDate))
        {
            throw new InvalidOperationException(
                $"Journal entry cannot be posted on {accountingDate} because the fiscal period does not contain that date.");
        }

        if (!fiscalPeriod.IsOpen)
        {
            throw new InvalidOperationException(
                $"Journal entry cannot be posted on {accountingDate} because the fiscal period is not open.");
        }
    }
}
