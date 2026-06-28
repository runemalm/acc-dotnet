using System.Collections.Concurrent;
using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

public sealed class InMemoryChartOfAccountsStore : IChartOfAccountsStore
{
    private readonly ConcurrentDictionary<Guid, ChartOfAccountsView> charts = new();

    public ChartOfAccountsView? Find(Guid chartOfAccountsId) =>
        charts.GetValueOrDefault(chartOfAccountsId);

    public ChartOfAccountsView? FindFor(Guid accountingSubjectId) =>
        charts.Values.SingleOrDefault(chart => chart.AccountingSubjectId == accountingSubjectId);

    public void Save(ChartOfAccountsView chartOfAccounts)
    {
        ArgumentNullException.ThrowIfNull(chartOfAccounts);
        charts[chartOfAccounts.ChartOfAccountsId] = chartOfAccounts;
    }
}
