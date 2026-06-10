using ACC.BuildingBlocks.EventSourcing;
using ACC.Ledger.Domain.Events;
using ACC.Ledger.Domain.Invariants;

namespace ACC.Ledger.Domain.Aggregates;

public sealed class JournalEntry : EventSourcedAggregate
{
    private JournalEntry()
    {
        Description = string.Empty;
        Lines = [];
    }

    private static void EnsureCanPost(
        Guid id,
        DateOnly accountingDate,
        string description,
        IReadOnlyCollection<JournalEntryLine> lines,
        FiscalPeriod? fiscalPeriod)
    {
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(lines);

        if (id == Guid.Empty)
        {
            throw new ArgumentException("A journal entry must have an identity.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("A journal entry must have a description.", nameof(description));
        }

        if (lines.Count == 0)
        {
            throw new ArgumentException("A journal entry must have at least one line.", nameof(lines));
        }

        if (lines.Any(line => line is null))
        {
            throw new ArgumentException("A journal entry cannot contain an empty line.", nameof(lines));
        }

        var journalEntry = new JournalEntry
        {
            Id = id,
            AccountingDate = accountingDate,
            Description = description,
            Lines = lines.ToArray()
        };

        JournalEntryMustBalance.Ensure(journalEntry);
        PostingMustOccurInOpenPeriod.Ensure(accountingDate, fiscalPeriod);
    }

    public Guid Id { get; private set; }

    public DateOnly AccountingDate { get; private set; }

    public string Description { get; private set; }

    public IReadOnlyCollection<JournalEntryLine> Lines { get; private set; }

    public decimal TotalDebits => Lines.Sum(line => line.Debit);

    public decimal TotalCredits => Lines.Sum(line => line.Credit);

    public static JournalEntry Posted(
        Guid id,
        DateOnly accountingDate,
        string description,
        IReadOnlyCollection<JournalEntryLine> lines,
        FiscalPeriod? fiscalPeriod,
        DateTimeOffset occurredAt)
    {
        EnsureCanPost(id, accountingDate, description, lines, fiscalPeriod);

        var journalEntry = new JournalEntry();
        journalEntry.Raise(new JournalEntryPosted(
            id,
            accountingDate,
            description,
            lines,
            occurredAt));

        return journalEntry;
    }

    public static JournalEntry Rehydrate(IEnumerable<object> events)
    {
        var journalEntry = new JournalEntry();
        journalEntry.LoadFromHistory(events);

        return journalEntry;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case JournalEntryPosted posted:
                Apply(posted);
                break;
        }
    }

    private void Apply(JournalEntryPosted domainEvent)
    {
        Id = domainEvent.JournalEntryId;
        AccountingDate = domainEvent.AccountingDate;
        Description = domainEvent.Description;
        Lines = domainEvent.Lines.ToArray();
    }
}
