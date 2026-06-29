using ACC.BuildingBlocks.Domain;

namespace ACC.Ledger.Domain.Invariants;

public static class PostingAccountMustBeRecognized
{
    public static void Ensure(
        string accountNumber,
        bool isRecognized)
    {
        if (!isRecognized)
        {
            throw new PostingAccountMustBeRecognizedViolation(accountNumber);
        }
    }
}

public sealed class PostingAccountMustBeRecognizedViolation(string accountNumber)
    : InvariantViolationException(
        $"Account {accountNumber} must be recognized by the accounting subject's operative chart before it can receive postings.");
