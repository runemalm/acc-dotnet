using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccountsTemplates;
using ACC.ChartOfAccounts.Tests.TestKit;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.UseCases;

public sealed class ViewChartOfAccountsTemplatesTests
{
    [Fact]
    public void GivenAvailableTemplates_WhenViewingTemplates_ThenTemplatesReturned()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        context.AddTemplate("first", "First chart");
        context.AddTemplate("second", "Second chart");

        var response = context.ViewChartOfAccountsTemplates.Handle(
            new ViewChartOfAccountsTemplatesQuery());

        Assert.Collection(
            response.Templates,
            template =>
            {
                Assert.Equal("first", template.TemplateId);
                Assert.Equal("First chart", template.Name);
                Assert.Equal(2, template.AccountCount);
            },
            template =>
            {
                Assert.Equal("second", template.TemplateId);
                Assert.Equal("Second chart", template.Name);
                Assert.Equal(2, template.AccountCount);
            });
    }
}
