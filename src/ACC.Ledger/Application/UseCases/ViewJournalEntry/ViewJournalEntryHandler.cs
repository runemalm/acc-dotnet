using ACC.Ledger.Application.Ports.ReadModels.JournalEntry;

namespace ACC.Ledger.Application.UseCases.ViewJournalEntry;

public sealed class ViewJournalEntryHandler
{
    private readonly IJournalEntryStore journalEntries;

    public ViewJournalEntryHandler(IJournalEntryStore journalEntries)
    {
        this.journalEntries = journalEntries;
    }

    public ViewJournalEntryResponse? Handle(ViewJournalEntryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var journalEntry = journalEntries.Find(query.JournalEntryId);

        return journalEntry is null
            ? null
            : new ViewJournalEntryResponse(
                journalEntry.JournalEntryId,
                journalEntry.AccountingDate,
                journalEntry.Description,
                journalEntry.Lines
                    .Select(line => new ViewJournalEntryResponseLine(line.Account, line.Debit, line.Credit))
                    .ToArray(),
                journalEntry.PostedAt);
    }
}
