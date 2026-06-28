using ACC.Ledger.Application.Ports.Authority;
using ACC.Ledger.Application.Ports.ReadModels.JournalEntry;
using ACC.Ledger.Domain.Invariants;

namespace ACC.Ledger.Application.UseCases.ViewJournalEntry;

public sealed class ViewJournalEntryHandler
{
    private readonly IJournalEntryStore journalEntries;
    private readonly ILedgerAuthorityPort authority;

    public ViewJournalEntryHandler(
        IJournalEntryStore journalEntries,
        ILedgerAuthorityPort authority)
    {
        this.journalEntries = journalEntries;
        this.authority = authority;
    }

    public ViewJournalEntryResponse? Handle(ViewJournalEntryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var journalEntry = journalEntries.Find(query.JournalEntryId);

        if (journalEntry is null)
        {
            return null;
        }

        ActorMustHaveLedgerPower.Ensure(
            authority.CanViewJournalEntry(query.ActorUserId, journalEntry.AccountingSubjectId),
            query.ActorUserId,
            "view a journal entry");

        return new ViewJournalEntryResponse(
            journalEntry.JournalEntryId,
            journalEntry.AccountingSubjectId,
            journalEntry.AccountingDate,
            journalEntry.Description,
            journalEntry.Lines
                .Select(line => new ViewJournalEntryResponseLine(line.Account, line.Debit, line.Credit))
                .ToArray(),
            journalEntry.PostedAt);
    }
}
