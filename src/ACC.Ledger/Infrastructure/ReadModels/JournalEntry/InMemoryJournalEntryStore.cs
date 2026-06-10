using System.Collections.Concurrent;
using ACC.Ledger.Application.Ports.ReadModels.JournalEntry;

namespace ACC.Ledger.Infrastructure.ReadModels.JournalEntry;

public sealed class InMemoryJournalEntryStore : IJournalEntryStore
{
    private readonly ConcurrentDictionary<Guid, JournalEntryView> journalEntries = new();

    public JournalEntryView? Find(Guid journalEntryId) =>
        journalEntries.GetValueOrDefault(journalEntryId);

    public void Save(JournalEntryView journalEntry)
    {
        ArgumentNullException.ThrowIfNull(journalEntry);

        journalEntries[journalEntry.JournalEntryId] = journalEntry;
    }
}
