using ACC.BuildingBlocks.EventSourcing;
using ACC.Ledger.Application.Ports.ChartOfAccounts;
using ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Domain.Events;
using ACC.Ledger.Domain.Invariants;
using ACC.Ledger.Infrastructure.ReadModels.JournalEntry;

namespace ACC.Ledger.Application.UseCases.PostJournalEntry;

public sealed class PostJournalEntryHandler
{
    private readonly EventSourcedRepository<JournalEntry> journalEntries;
    private readonly EventSourcedRepository<FiscalPeriod> fiscalPeriods;
    private readonly IFiscalPeriodStore fiscalPeriodStore;
    private readonly IAccountAvailabilityPort accountAvailability;
    private readonly JournalEntryProjection journalEntryProjection;

    public PostJournalEntryHandler(
        EventSourcedRepository<JournalEntry> journalEntries,
        EventSourcedRepository<FiscalPeriod> fiscalPeriods,
        IFiscalPeriodStore fiscalPeriodStore,
        IAccountAvailabilityPort accountAvailability,
        JournalEntryProjection journalEntryProjection)
    {
        this.journalEntries = journalEntries;
        this.fiscalPeriods = fiscalPeriods;
        this.fiscalPeriodStore = fiscalPeriodStore;
        this.accountAvailability = accountAvailability;
        this.journalEntryProjection = journalEntryProjection;
    }

    public PostJournalEntryResult Handle(PostJournalEntryCommand command, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lines = command.Lines
                .Select(line => new JournalEntryLine(line.Account, line.Debit, line.Credit))
                .ToArray();

        var fiscalPeriodView = fiscalPeriodStore.FindFor(
            command.AccountingSubjectId,
            command.AccountingDate)
            ?? throw new InvalidOperationException(
                $"No fiscal period contains accounting date {command.AccountingDate}.");

        var fiscalPeriod = fiscalPeriods.Load(FiscalPeriodStream(fiscalPeriodView.FiscalPeriodId));

        foreach (var accountNumber in lines
                     .Select(line => line.Account)
                     .Distinct(StringComparer.Ordinal))
        {
            var availability = accountAvailability.GetAvailability(
                command.AccountingSubjectId,
                accountNumber);
            PostingAccountMustBeRecognized.Ensure(
                accountNumber,
                availability != PostingAccountAvailability.Unrecognized);
            PostingAccountMustBeActive.Ensure(
                accountNumber,
                availability == PostingAccountAvailability.Active);
        }

        var journalEntryId = Guid.NewGuid();

        var journalEntry = JournalEntry.Posted(
            journalEntryId,
            command.AccountingDate,
            command.Description,
            lines,
            fiscalPeriod,
            occurredAt);

        var storedEvents = journalEntries.Save(JournalEntryStream(journalEntryId), journalEntry);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<JournalEntryPosted>()
            .Single();

        journalEntryProjection.Project(domainEvent);

        return new PostJournalEntryResult(domainEvent.JournalEntryId);
    }

    private static StreamId JournalEntryStream(Guid journalEntryId) =>
        StreamId.For("journal-entry", journalEntryId);

    private static StreamId FiscalPeriodStream(Guid fiscalPeriodId) =>
        StreamId.For("fiscal-period", fiscalPeriodId);
}
