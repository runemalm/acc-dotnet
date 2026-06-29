using ACC.BuildingBlocks.Domain;

namespace ACC.Ledger.Domain.Invariants;

public static class PostingAccountMustBeActive
{
    public static void Ensure(
        string accountNumber,
        bool isActive)
    {
        if (!isActive)
        {
            throw new PostingAccountMustBeActiveViolation(accountNumber);
        }
    }
}

public sealed class PostingAccountMustBeActiveViolation(string accountNumber)
    : InvariantViolationException(
        $"Account {accountNumber} must be active in the accounting subject's operative chart before it can receive postings.");
