using ACC.BuildingBlocks.Domain;

namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class AccountingSubjectMustHaveAtMostOneOperativeChartOfAccounts
{
    public static void Ensure(bool hasNoOperativeChart, Guid accountingSubjectId)
    {
        if (!hasNoOperativeChart)
        {
            throw new AccountingSubjectMustHaveAtMostOneOperativeChartOfAccountsViolation(
                accountingSubjectId);
        }
    }
}

public sealed class AccountingSubjectMustHaveAtMostOneOperativeChartOfAccountsViolation(
    Guid accountingSubjectId)
    : InvariantViolationException(
        $"Accounting subject {accountingSubjectId} already has an operative chart of accounts.");
