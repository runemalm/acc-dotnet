using ACC.BuildingBlocks.Authorization;
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
            new ViewChartOfAccountsQuery(actorUserId, accountingSubjectId));

        Assert.NotNull(response);
        Assert.Equal(chartOfAccountsId, response.ChartOfAccountsId);
        Assert.Equal(accountingSubjectId, response.AccountingSubjectId);
        Assert.Equal("test-template", response.Template.Id);
        Assert.Equal("Test chart of accounts", response.Template.Name);
        Assert.Equal(2, response.Accounts.Count);
    }

    [Fact]
    public void GivenActorWithoutViewChartOfAccountsPower_WhenViewing_ThenAuthorizationDenied()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var adoptingActorUserId = Guid.NewGuid();
        var viewingActorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        context.AdoptChart(accountingSubjectId, adoptingActorUserId);

        var exception = Assert.Throws<AuthorizationDeniedException>(() =>
            context.ViewChartOfAccounts.Handle(
                new ViewChartOfAccountsQuery(viewingActorUserId, accountingSubjectId)));

        Assert.Contains("must have power to view a chart of accounts", exception.Message);
    }
}
