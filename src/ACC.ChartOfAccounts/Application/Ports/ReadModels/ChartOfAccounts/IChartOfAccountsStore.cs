namespace ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

public interface IChartOfAccountsStore
{
    ChartOfAccountsView? Find(Guid chartOfAccountsId);

    ChartOfAccountsView? FindFor(Guid accountingSubjectId);

    void Save(ChartOfAccountsView chartOfAccounts);
}
