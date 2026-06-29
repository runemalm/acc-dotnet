using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.Ledger.Application.Ports.ChartOfAccounts;
using ACC.Ledger.Application.Ports.Authority;
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
    private readonly ILedgerAuthorityPort authority;
    private readonly JournalEntryProjection journalEntryProjection;

    public PostJournalEntryHandler(
        EventSourcedRepository<JournalEntry> journalEntries,
        EventSourcedRepository<FiscalPeriod> fiscalPeriods,
        IFiscalPeriodStore fiscalPeriodStore,
        IAccountAvailabilityPort accountAvailability,
        ILedgerAuthorityPort authority,
        JournalEntryProjection journalEntryProjection)
    {
        this.journalEntries = journalEntries;
        this.fiscalPeriods = fiscalPeriods;
        this.fiscalPeriodStore = fiscalPeriodStore;
        this.accountAvailability = accountAvailability;
        this.authority = authority;
        this.journalEntryProjection = journalEntryProjection;
    }

    public PostJournalEntryResult Handle(PostJournalEntryCommand command, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        ActorMustHaveLedgerPower.Ensure(
            authority.CanPostJournalEntry(command.ActorUserId, command.AccountingSubjectId),
            command.ActorUserId,
            "post a journal entry");

        var lines = command.Lines
                .Select(line => new JournalEntryLine(line.Account, line.Debit, line.Credit))
                .ToArray();

        var fiscalPeriodView = fiscalPeriodStore.FindFor(
            command.AccountingSubjectId,
            command.AccountingDate);

        var fiscalPeriod = fiscalPeriodView is null
            ? null
            : fiscalPeriods.Load(FiscalPeriodStream(fiscalPeriodView.FiscalPeriodId));

        var journalEntryId = Guid.NewGuid();
        var journalEntry = JournalEntry.Posted(
            journalEntryId,
            command.AccountingSubjectId,
            command.AccountingDate,
            command.Description,
            lines,
            fiscalPeriod,
            occurredAt);

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

    private static void ValidateCommand(PostJournalEntryCommand command)
    {
        if (command.ActorUserId == Guid.Empty || command.AccountingSubjectId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Posting a journal entry must identify the acting user and accounting subject.");
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            throw new ApplicationValidationException(
                "A journal entry must have a description.");
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            throw new ApplicationValidationException(
                "A journal entry must have at least one line.");
        }

        foreach (var line in command.Lines)
        {
            if (line is null || string.IsNullOrWhiteSpace(line.Account))
            {
                throw new ApplicationValidationException(
                    "A journal entry line must name an account.");
            }

            if (line.Debit < 0 || line.Credit < 0)
            {
                throw new ApplicationValidationException(
                    "A journal entry line amount cannot be negative.");
            }

            if (line.Debit == 0 && line.Credit == 0)
            {
                throw new ApplicationValidationException(
                    "A journal entry line must carry either a debit or a credit.");
            }

            if (line.Debit > 0 && line.Credit > 0)
            {
                throw new ApplicationValidationException(
                    "A journal entry line cannot carry both a debit and a credit.");
            }
        }
    }
}
