using ACC.BuildingBlocks.Failures;
using ACC.Ledger.Domain.Aggregates;

namespace ACC.Ledger.Domain.Invariants;

public static class FiscalPeriodMustBeOpenToClose
{
    public static void Ensure(FiscalPeriod fiscalPeriod)
    {
        ArgumentNullException.ThrowIfNull(fiscalPeriod);

        if (!fiscalPeriod.IsOpen)
        {
            throw new StateConflictException("A fiscal period must be open before it can be closed.");
        }
    }
}
