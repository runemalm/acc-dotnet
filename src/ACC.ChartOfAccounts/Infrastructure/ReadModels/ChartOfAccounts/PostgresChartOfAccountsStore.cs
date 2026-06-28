using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

public sealed class PostgresChartOfAccountsStore : IChartOfAccountsStore
{
    public ChartOfAccountsView? Find(Guid chartOfAccountsId) =>
        throw new NotSupportedException("Postgres chart of accounts persistence has not been implemented yet.");

    public ChartOfAccountsView? FindFor(Guid accountingSubjectId) =>
        throw new NotSupportedException("Postgres chart of accounts persistence has not been implemented yet.");

    public void Save(ChartOfAccountsView chartOfAccounts) =>
        throw new NotSupportedException("Postgres chart of accounts persistence has not been implemented yet.");
}
