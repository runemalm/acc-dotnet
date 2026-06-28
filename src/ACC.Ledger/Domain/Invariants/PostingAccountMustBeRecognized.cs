using ACC.BuildingBlocks.Failures;

namespace ACC.Ledger.Domain.Invariants;

public static class PostingAccountMustBeRecognized
{
    public static void Ensure(
        string accountNumber,
        bool isRecognized)
    {
        if (!isRecognized)
        {
            throw new SemanticViolationException(
                $"Account {accountNumber} must be recognized by the accounting subject's operative chart before it can receive postings.");
        }
    }
}
