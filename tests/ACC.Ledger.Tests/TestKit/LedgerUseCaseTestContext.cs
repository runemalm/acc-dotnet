using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;
using ACC.Ledger.Application.Ports.ReadModels.JournalEntry;
using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;
using ACC.Ledger.Infrastructure.ReadModels.JournalEntry;

namespace ACC.Ledger.Tests.TestKit;

internal sealed class LedgerUseCaseTestContext
{
    private readonly InMemoryFiscalPeriodStore fiscalPeriodStore = new();
    private readonly InMemoryJournalEntryStore journalEntryStore = new();

    public LedgerUseCaseTestContext()
    {
        var eventStore = new InMemoryEventStore();

        var fiscalPeriods = new EventSourcedRepository<FiscalPeriod>(
            eventStore,
            FiscalPeriod.Rehydrate);

        var journalEntries = new EventSourcedRepository<JournalEntry>(
            eventStore,
            JournalEntry.Rehydrate);

        var fiscalPeriodProjection = new FiscalPeriodProjection(fiscalPeriodStore);
        var journalEntryProjection = new JournalEntryProjection(journalEntryStore);

        OpenFiscalPeriod = new OpenFiscalPeriodHandler(
            fiscalPeriods,
            fiscalPeriodProjection);

        CloseFiscalPeriod = new CloseFiscalPeriodHandler(
            fiscalPeriods,
            fiscalPeriodProjection);

        PostJournalEntry = new PostJournalEntryHandler(
            journalEntries,
            fiscalPeriods,
            fiscalPeriodStore,
            journalEntryProjection);
    }

    public OpenFiscalPeriodHandler OpenFiscalPeriod { get; }

    public CloseFiscalPeriodHandler CloseFiscalPeriod { get; }

    public PostJournalEntryHandler PostJournalEntry { get; }

    public FiscalPeriodView? FindFiscalPeriod(Guid fiscalPeriodId) =>
        fiscalPeriodStore.Find(fiscalPeriodId);

    public FiscalPeriodView? FindFiscalPeriodFor(Guid accountingSubjectId, DateOnly accountingDate) =>
        fiscalPeriodStore.FindFor(accountingSubjectId, accountingDate);

    public JournalEntryView? FindJournalEntry(Guid journalEntryId) =>
        journalEntryStore.Find(journalEntryId);
}
