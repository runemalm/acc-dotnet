using ACC.BuildingBlocks.Failures;

namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class AccountingSubjectMustHaveAtMostOneOperativeChartOfAccounts
{
    public static void Ensure(bool hasNoOperativeChart, Guid accountingSubjectId)
    {
        if (!hasNoOperativeChart)
        {
            throw new StateConflictException(
                $"Accounting subject {accountingSubjectId} already has an operative chart of accounts.");
        }
    }
}
