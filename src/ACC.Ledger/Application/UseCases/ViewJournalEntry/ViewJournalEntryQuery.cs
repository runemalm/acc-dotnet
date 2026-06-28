namespace ACC.Ledger.Application.UseCases.ViewJournalEntry;

public sealed record ViewJournalEntryQuery(
    Guid ActorUserId,
    Guid JournalEntryId);
