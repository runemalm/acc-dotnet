namespace ACC.ChartOfAccounts.Application.Ports.Authority;

public interface IChartOfAccountsAuthorityPort
{
    bool CanAdoptChartOfAccounts(Guid actorUserId, Guid accountingSubjectId);

    bool CanManageChartOfAccounts(Guid actorUserId, Guid accountingSubjectId);

    bool CanViewChartOfAccounts(Guid actorUserId, Guid accountingSubjectId);
}
