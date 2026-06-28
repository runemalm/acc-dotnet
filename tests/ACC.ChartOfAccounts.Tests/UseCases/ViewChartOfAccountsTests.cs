using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;
using ACC.ChartOfAccounts.Tests.TestKit;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.UseCases;

public sealed class ViewChartOfAccountsTests
{
    [Fact]
    public void GivenAdoptedChart_WhenViewingChart_ThenChartAndAccountsReturned()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var response = context.ViewChartOfAccounts.Handle(
            new ViewChartOfAccountsQuery(accountingSubjectId));

        Assert.NotNull(response);
        Assert.Equal(chartOfAccountsId, response.ChartOfAccountsId);
        Assert.Equal(accountingSubjectId, response.AccountingSubjectId);
        Assert.Equal("test-template", response.Template.Id);
        Assert.Equal("Test chart of accounts", response.Template.Name);
        Assert.Equal(2, response.Accounts.Count);
    }
}
