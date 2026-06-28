using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;

public sealed class ViewChartOfAccountsHandler
{
    private readonly IChartOfAccountsStore charts;
    private readonly IAccountStore accounts;

    public ViewChartOfAccountsHandler(
        IChartOfAccountsStore charts,
        IAccountStore accounts)
    {
        this.charts = charts;
        this.accounts = accounts;
    }

    public ViewChartOfAccountsResponse? Handle(ViewChartOfAccountsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var chart = charts.FindFor(query.AccountingSubjectId);

        return chart is null
            ? null
            : new ViewChartOfAccountsResponse(
                chart.ChartOfAccountsId,
                chart.AccountingSubjectId,
                new AdoptedChartOfAccountsTemplateResponse(
                    chart.Template.Id,
                    chart.Template.Name),
                chart.AdoptedAt,
                accounts.FindFor(chart.ChartOfAccountsId)
                    .Select(account => new ViewAccountResponse(
                        account.Number,
                        account.Name,
                        account.IsActive))
                    .ToArray());
    }
}
