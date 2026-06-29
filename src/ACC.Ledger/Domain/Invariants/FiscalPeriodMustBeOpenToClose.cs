using ACC.Ledger.Domain.Aggregates;
using ACC.BuildingBlocks.Domain;

namespace ACC.Ledger.Domain.Invariants;

public static class FiscalPeriodMustBeOpenToClose
{
    public static void Ensure(FiscalPeriod fiscalPeriod)
    {
        ArgumentNullException.ThrowIfNull(fiscalPeriod);

        if (!fiscalPeriod.IsOpen)
        {
            throw new FiscalPeriodMustBeOpenToCloseViolation();
        }
    }
}

public sealed class FiscalPeriodMustBeOpenToCloseViolation()
    : InvariantViolationException("A fiscal period must be open before it can be closed.");
