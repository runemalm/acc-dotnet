namespace ACC.Ledger.Application.Ports.Authority;

public interface ILedgerAuthorityPort
{
    bool CanPostJournalEntry(Guid actorUserId, Guid accountingSubjectId);

    bool CanViewJournalEntry(Guid actorUserId, Guid accountingSubjectId);

    bool CanOpenFiscalPeriod(Guid actorUserId, Guid accountingSubjectId);

    bool CanCloseFiscalPeriod(Guid actorUserId, Guid accountingSubjectId);
}
