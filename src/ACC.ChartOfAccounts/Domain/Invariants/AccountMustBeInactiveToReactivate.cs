using ACC.BuildingBlocks.Domain;
using ACC.ChartOfAccounts.Domain.Aggregates;

namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class AccountMustBeInactiveToReactivate
{
    public static void Ensure(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (account.IsActive)
        {
            throw new AccountMustBeInactiveToReactivateViolation(account.Number);
        }
    }
}

public sealed class AccountMustBeInactiveToReactivateViolation(string accountNumber)
    : InvariantViolationException($"Account {accountNumber} is already active.");
