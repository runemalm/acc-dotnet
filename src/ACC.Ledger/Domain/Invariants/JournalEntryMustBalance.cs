using ACC.Ledger.Domain.Aggregates;
using ACC.BuildingBlocks.Domain;

namespace ACC.Ledger.Domain.Invariants;

public static class JournalEntryMustBalance
{
    public static void Ensure(JournalEntry journalEntry)
    {
        if (journalEntry.TotalDebits != journalEntry.TotalCredits)
        {
            throw new JournalEntryMustBalanceViolation(
                journalEntry.TotalDebits,
                journalEntry.TotalCredits);
        }
    }
}

public sealed class JournalEntryMustBalanceViolation(decimal debits, decimal credits)
    : InvariantViolationException(
        $"Journal entry must balance. Debits: {debits}; credits: {credits}.");
