using ACC.Ledger.Application.Ports.Authority;

namespace ACC.Ledger.Tests.TestKit;

internal sealed class TestLedgerAuthorityPort(bool grantsAllByDefault = false)
    : ILedgerAuthorityPort
{
    private readonly HashSet<(Guid ActorUserId, Guid AccountingSubjectId)> postingPowers = [];
    private readonly HashSet<(Guid ActorUserId, Guid AccountingSubjectId)> viewingPowers = [];
    private readonly HashSet<(Guid ActorUserId, Guid AccountingSubjectId)> deniedViewingPowers = [];
    private readonly HashSet<(Guid ActorUserId, Guid AccountingSubjectId)> openingPowers = [];
    private readonly HashSet<(Guid ActorUserId, Guid AccountingSubjectId)> closingPowers = [];

    public bool CanPostJournalEntry(Guid actorUserId, Guid accountingSubjectId) =>
        grantsAllByDefault || postingPowers.Contains((actorUserId, accountingSubjectId));

    public bool CanViewJournalEntry(Guid actorUserId, Guid accountingSubjectId) =>
        !deniedViewingPowers.Contains((actorUserId, accountingSubjectId)) &&
        (grantsAllByDefault || viewingPowers.Contains((actorUserId, accountingSubjectId)));

    public bool CanOpenFiscalPeriod(Guid actorUserId, Guid accountingSubjectId) =>
        grantsAllByDefault || openingPowers.Contains((actorUserId, accountingSubjectId));

    public bool CanCloseFiscalPeriod(Guid actorUserId, Guid accountingSubjectId) =>
        grantsAllByDefault || closingPowers.Contains((actorUserId, accountingSubjectId));

    public void AllowPosting(Guid actorUserId, Guid accountingSubjectId) =>
        postingPowers.Add((actorUserId, accountingSubjectId));

    public void AllowViewing(Guid actorUserId, Guid accountingSubjectId) =>
        viewingPowers.Add((actorUserId, accountingSubjectId));

    public void DenyViewing(Guid actorUserId, Guid accountingSubjectId) =>
        deniedViewingPowers.Add((actorUserId, accountingSubjectId));

    public void AllowOpening(Guid actorUserId, Guid accountingSubjectId) =>
        openingPowers.Add((actorUserId, accountingSubjectId));

    public void AllowClosing(Guid actorUserId, Guid accountingSubjectId) =>
        closingPowers.Add((actorUserId, accountingSubjectId));

    public void AllowAll(Guid actorUserId, Guid accountingSubjectId)
    {
        AllowPosting(actorUserId, accountingSubjectId);
        AllowViewing(actorUserId, accountingSubjectId);
        AllowOpening(actorUserId, accountingSubjectId);
        AllowClosing(actorUserId, accountingSubjectId);
    }
}
