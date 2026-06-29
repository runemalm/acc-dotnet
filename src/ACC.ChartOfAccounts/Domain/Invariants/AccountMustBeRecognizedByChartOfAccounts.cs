using ACC.BuildingBlocks.Domain;

namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class AccountMustBeRecognizedByChartOfAccounts
{
    public static void Ensure(bool isRecognized, string accountNumber)
    {
        if (!isRecognized)
        {
            throw new AccountMustBeRecognizedByChartOfAccountsViolation(accountNumber);
        }
    }
}

public sealed class AccountMustBeRecognizedByChartOfAccountsViolation(string accountNumber)
    : InvariantViolationException(
        $"Account {accountNumber} is not recognized by the chart of accounts.");
