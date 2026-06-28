using ACC.BuildingBlocks.Failures;
using ACC.ChartOfAccounts.Domain.Aggregates;

namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class AccountNumberMustBeUniqueWithinChartOfAccounts
{
    public static void Ensure(IEnumerable<Account> accounts, string accountNumber)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);

        if (accounts.Any(account => account.Number == accountNumber))
        {
            throw new StateConflictException(
                $"Account number {accountNumber} must be unique within the chart of accounts.");
        }
    }

    public static void Ensure(IReadOnlyCollection<Account> accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts);

        var duplicate = accounts
            .GroupBy(account => account.Number)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new StateConflictException(
                $"Account number {duplicate.Key} must be unique within the chart of accounts.");
        }
    }
}
