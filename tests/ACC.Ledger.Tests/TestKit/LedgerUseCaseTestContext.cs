using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;
using ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;
using ACC.Ledger.Application.Ports.ReadModels.JournalEntry;
using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Application.UseCases.ViewJournalEntry;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;
using ACC.Ledger.Infrastructure.ReadModels.JournalEntry;
using ACC.Ledger.Infrastructure.Adapters.ChartOfAccounts;

namespace ACC.Ledger.Tests.TestKit;

internal sealed class LedgerUseCaseTestContext
{
    private readonly InMemoryFiscalPeriodStore fiscalPeriodStore = new();
    private readonly InMemoryJournalEntryStore journalEntryStore = new();
    private readonly InMemoryAccountStore accountStore = new();
    private readonly TestLedgerAuthorityPort authority = new();

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
        var accountAvailability = new AccountAvailabilityAdapter(accountStore);

        OpenFiscalPeriod = new OpenFiscalPeriodHandler(
            fiscalPeriods,
            fiscalPeriodProjection,
            authority);

        CloseFiscalPeriod = new CloseFiscalPeriodHandler(
            fiscalPeriods,
            fiscalPeriodProjection,
            authority);

        PostJournalEntry = new PostJournalEntryHandler(
            journalEntries,
            fiscalPeriods,
            fiscalPeriodStore,
            accountAvailability,
            authority,
            journalEntryProjection);

        ViewJournalEntry = new ViewJournalEntryHandler(
            journalEntryStore,
            authority);
    }

    public OpenFiscalPeriodHandler OpenFiscalPeriod { get; }

    public CloseFiscalPeriodHandler CloseFiscalPeriod { get; }

    public PostJournalEntryHandler PostJournalEntry { get; }

    public ViewJournalEntryHandler ViewJournalEntry { get; }

    public FiscalPeriodView? FindFiscalPeriod(Guid fiscalPeriodId) =>
        fiscalPeriodStore.Find(fiscalPeriodId);

    public FiscalPeriodView? FindFiscalPeriodFor(Guid accountingSubjectId, DateOnly accountingDate) =>
        fiscalPeriodStore.FindFor(accountingSubjectId, accountingDate);

    public JournalEntryView? FindJournalEntry(Guid journalEntryId) =>
        journalEntryStore.Find(journalEntryId);

    public void AllowAllLedgerActs(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowAll(actorUserId, accountingSubjectId);

    public void AllowOpeningFiscalPeriod(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowOpening(actorUserId, accountingSubjectId);

    public void AllowClosingFiscalPeriod(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowClosing(actorUserId, accountingSubjectId);

    public void AllowPostingJournalEntry(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowPosting(actorUserId, accountingSubjectId);

    public void AllowViewingJournalEntry(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowViewing(actorUserId, accountingSubjectId);

    public void MakeAccountsActive(Guid accountingSubjectId, params string[] accountNumbers)
    {
        foreach (var accountNumber in accountNumbers)
        {
            accountStore.Save(new AccountView(
                Guid.NewGuid(),
                accountingSubjectId,
                accountNumber,
                accountNumber,
                IsActive: true));
        }
    }

    public void MakeAccountInactive(Guid accountingSubjectId, string accountNumber) =>
        accountStore.Save(new AccountView(
            Guid.NewGuid(),
            accountingSubjectId,
            accountNumber,
            accountNumber,
            IsActive: false));
}
