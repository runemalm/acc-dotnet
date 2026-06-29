using ACC.BuildingBlocks.Domain;

namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class AccountingSubjectMustBeRecognizedForChartOfAccounts
{
    public static void Ensure(bool isRecognized, Guid accountingSubjectId)
    {
        if (!isRecognized)
        {
            throw new AccountingSubjectMustBeRecognizedForChartOfAccountsViolation(
                accountingSubjectId);
        }
    }
}

public sealed class AccountingSubjectMustBeRecognizedForChartOfAccountsViolation(
    Guid accountingSubjectId)
    : InvariantViolationException(
        $"Accounting subject {accountingSubjectId} must be recognized before adopting a chart of accounts.");
