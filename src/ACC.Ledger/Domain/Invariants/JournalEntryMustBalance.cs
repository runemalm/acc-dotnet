using ACC.BuildingBlocks.Failures;
using ACC.Ledger.Domain.Aggregates;

namespace ACC.Ledger.Domain.Invariants;

public static class JournalEntryMustBalance
{
    public static void Ensure(JournalEntry journalEntry)
    {
        if (journalEntry.TotalDebits != journalEntry.TotalCredits)
        {
            throw new SemanticViolationException(
                $"Journal entry must balance. Debits: {journalEntry.TotalDebits}; credits: {journalEntry.TotalCredits}.");
        }
    }
}
