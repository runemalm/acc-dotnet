using ACC.ChartOfAccounts.Application.Ports.Authority;
using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Domain.Invariants;

namespace ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;

public sealed class ViewChartOfAccountsHandler
{
    private readonly IChartOfAccountsStore charts;
    private readonly IAccountStore accounts;
    private readonly IChartOfAccountsAuthorityPort authority;

    public ViewChartOfAccountsHandler(
        IChartOfAccountsStore charts,
        IAccountStore accounts,
        IChartOfAccountsAuthorityPort authority)
    {
        this.charts = charts;
        this.accounts = accounts;
        this.authority = authority;
    }

    public ViewChartOfAccountsResponse? Handle(ViewChartOfAccountsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var chart = charts.FindFor(query.AccountingSubjectId);

        if (chart is null)
        {
            return null;
        }

        ActorMustHaveChartOfAccountsPower.Ensure(
            authority.CanViewChartOfAccounts(query.ActorUserId, chart.AccountingSubjectId),
            query.ActorUserId,
            "view a chart of accounts");

        return new ViewChartOfAccountsResponse(
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
