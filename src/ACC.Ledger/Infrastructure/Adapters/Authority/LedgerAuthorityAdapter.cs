using ACC.Authority.Application.Policies;
using ACC.Authority.Domain.Powers;
using ACC.Ledger.Application.Ports.Authority;

namespace ACC.Ledger.Infrastructure.Adapters.Authority;

public sealed class LedgerAuthorityAdapter : ILedgerAuthorityPort
{
    private readonly IAuthorityPolicy authorityPolicy;

    public LedgerAuthorityAdapter(IAuthorityPolicy authorityPolicy)
    {
        this.authorityPolicy = authorityPolicy;
    }

    public bool CanPostJournalEntry(Guid actorUserId, Guid accountingSubjectId) =>
        authorityPolicy.HasPower(actorUserId, accountingSubjectId, Power.PostJournalEntry);

    public bool CanViewJournalEntry(Guid actorUserId, Guid accountingSubjectId) =>
        authorityPolicy.HasPower(actorUserId, accountingSubjectId, Power.ViewJournalEntry);

    public bool CanOpenFiscalPeriod(Guid actorUserId, Guid accountingSubjectId) =>
        authorityPolicy.HasPower(actorUserId, accountingSubjectId, Power.OpenFiscalPeriod);

    public bool CanCloseFiscalPeriod(Guid actorUserId, Guid accountingSubjectId) =>
        authorityPolicy.HasPower(actorUserId, accountingSubjectId, Power.CloseFiscalPeriod);
}
