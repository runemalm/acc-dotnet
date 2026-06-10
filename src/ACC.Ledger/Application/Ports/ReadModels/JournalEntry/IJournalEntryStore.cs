namespace ACC.Ledger.Application.Ports.ReadModels.JournalEntry;

public interface IJournalEntryStore
{
    JournalEntryView? Find(Guid journalEntryId);

    void Save(JournalEntryView journalEntry);
}
