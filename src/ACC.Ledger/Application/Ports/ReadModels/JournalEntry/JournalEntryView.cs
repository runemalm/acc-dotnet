namespace ACC.Ledger.Application.Ports.ReadModels.JournalEntry;

public sealed record JournalEntryView(
    Guid JournalEntryId,
    Guid AccountingSubjectId,
    DateOnly AccountingDate,
    string Description,
    IReadOnlyCollection<JournalEntryViewLine> Lines,
    DateTimeOffset PostedAt);

public sealed record JournalEntryViewLine(string Account, decimal Debit, decimal Credit);
