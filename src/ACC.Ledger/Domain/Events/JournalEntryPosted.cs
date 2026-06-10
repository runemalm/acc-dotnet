using ACC.Ledger.Domain.Aggregates;

namespace ACC.Ledger.Domain.Events;

public sealed record JournalEntryPosted
{
    public JournalEntryPosted(
        Guid journalEntryId,
        DateOnly accountingDate,
        string description,
        IReadOnlyCollection<JournalEntryLine> lines,
        DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(lines);

        if (journalEntryId == Guid.Empty)
        {
            throw new ArgumentException("A journal entry posted fact must identify the journal entry.", nameof(journalEntryId));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("A journal entry posted fact must describe the journal entry.", nameof(description));
        }

        if (lines.Count == 0)
        {
            throw new ArgumentException("A journal entry posted fact must include the posted lines.", nameof(lines));
        }

        JournalEntryId = journalEntryId;
        AccountingDate = accountingDate;
        Description = description;
        Lines = lines.ToArray();
        OccurredAt = occurredAt;
    }

    public Guid JournalEntryId { get; }

    public DateOnly AccountingDate { get; }

    public string Description { get; }

    public IReadOnlyCollection<JournalEntryLine> Lines { get; }

    public DateTimeOffset OccurredAt { get; }

    public static JournalEntryPosted From(JournalEntry journalEntry, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(journalEntry);

        return new JournalEntryPosted(
            journalEntry.Id,
            journalEntry.AccountingDate,
            journalEntry.Description,
            journalEntry.Lines,
            occurredAt);
    }
}
