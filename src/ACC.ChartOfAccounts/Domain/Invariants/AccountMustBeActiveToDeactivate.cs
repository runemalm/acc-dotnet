using ACC.BuildingBlocks.Domain;
using ACC.ChartOfAccounts.Domain.Aggregates;

namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class AccountMustBeActiveToDeactivate
{
    public static void Ensure(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (!account.IsActive)
        {
            throw new AccountMustBeActiveToDeactivateViolation(account.Number);
        }
    }
}

public sealed class AccountMustBeActiveToDeactivateViolation(string accountNumber)
    : InvariantViolationException($"Account {accountNumber} is already inactive.");
