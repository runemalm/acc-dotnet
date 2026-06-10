namespace ACC.Ledger.Application.UseCases.PostJournalEntry;

public sealed record PostJournalEntryCommand(
    Guid AccountingSubjectId,
    DateOnly AccountingDate,
    string Description,
    IReadOnlyCollection<PostJournalEntryCommandLine> Lines);

public sealed record PostJournalEntryCommandLine(string Account, decimal Debit, decimal Credit);
