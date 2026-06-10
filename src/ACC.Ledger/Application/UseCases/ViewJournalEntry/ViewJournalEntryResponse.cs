namespace ACC.Ledger.Application.UseCases.ViewJournalEntry;

public sealed record ViewJournalEntryResponse(
    Guid JournalEntryId,
    DateOnly AccountingDate,
    string Description,
    IReadOnlyCollection<ViewJournalEntryResponseLine> Lines,
    DateTimeOffset PostedAt);

public sealed record ViewJournalEntryResponseLine(string Account, decimal Debit, decimal Credit);
