using System.Net;
using System.Net.Http.Json;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccountsTemplates;
using ACC.ChartOfAccounts.Tests.TestKit;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.Api;

public sealed class ViewChartOfAccountsTemplatesEndpointTests
{
    [Fact]
    public async Task ViewChartOfAccountsTemplates_WithAvailableTemplates_ReturnsOk()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        context.AddTemplate();

        var response = await context.Client.GetAsync("/chart-of-accounts/templates");

        var result = await response.Content
            .ReadFromJsonAsync<ViewChartOfAccountsTemplatesResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        var template = Assert.Single(result.Templates);
        Assert.Equal("test-template", template.TemplateId);
        Assert.Equal("Test chart of accounts", template.Name);
        Assert.Equal(2, template.AccountCount);
    }
}
