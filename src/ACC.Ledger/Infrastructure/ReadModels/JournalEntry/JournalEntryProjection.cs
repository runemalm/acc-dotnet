using ACC.Ledger.Application.Ports.ReadModels.JournalEntry;
using ACC.Ledger.Domain.Events;

namespace ACC.Ledger.Infrastructure.ReadModels.JournalEntry;

public sealed class JournalEntryProjection
{
    private readonly IJournalEntryStore journalEntries;

    public JournalEntryProjection(IJournalEntryStore journalEntries)
    {
        this.journalEntries = journalEntries;
    }

    public void Project(JournalEntryPosted domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        journalEntries.Save(new JournalEntryView(
            domainEvent.JournalEntryId,
            domainEvent.AccountingSubjectId,
            domainEvent.AccountingDate,
            domainEvent.Description,
            domainEvent.Lines
                .Select(line => new JournalEntryViewLine(line.Account, line.Debit, line.Credit))
                .ToArray(),
            domainEvent.OccurredAt));
    }
}
