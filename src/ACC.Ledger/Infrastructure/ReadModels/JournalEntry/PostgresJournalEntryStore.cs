using ACC.Ledger.Application.Ports.ReadModels.JournalEntry;

namespace ACC.Ledger.Infrastructure.ReadModels.JournalEntry;

internal sealed class PostgresJournalEntryStore : IJournalEntryStore
{
    public JournalEntryView? Find(Guid journalEntryId) =>
        throw new NotImplementedException("Postgres journal entry read model has not been implemented yet.");

    public void Save(JournalEntryView journalEntry) =>
        throw new NotImplementedException("Postgres journal entry read model has not been implemented yet.");
}
