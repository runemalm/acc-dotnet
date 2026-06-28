using System.Net;
using System.Net.Http.Json;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;
using ACC.ChartOfAccounts.Tests.TestKit;
using ACC.Testing.Authentication;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.Api;

public sealed class ViewChartOfAccountsEndpointTests
{
    [Fact]
    public async Task ViewChartOfAccounts_WithAdoptedChart_ReturnsOk()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);
        context.Client.AuthenticateAs(actorUserId);

        var response = await context.Client.GetAsync(
            $"/chart-of-accounts/accounting-subjects/{accountingSubjectId}");

        var result = await response.Content.ReadFromJsonAsync<ViewChartOfAccountsResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(chartOfAccountsId, result.ChartOfAccountsId);
        Assert.Equal(accountingSubjectId, result.AccountingSubjectId);
        Assert.Equal("test-template", result.Template.Id);
        Assert.Equal(2, result.Accounts.Count);
    }

    [Fact]
    public async Task ViewChartOfAccounts_WithUnknownChart_ReturnsNotFound()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();

        var response = await context.Client.GetAsync(
            $"/chart-of-accounts/accounting-subjects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ViewChartOfAccounts_WithoutViewPower_ReturnsForbidden()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var adoptingActorUserId = Guid.NewGuid();
        context.AdoptChart(accountingSubjectId, adoptingActorUserId);
        context.Client.AuthenticateAs(Guid.NewGuid());

        var response = await context.Client.GetAsync(
            $"/chart-of-accounts/accounting-subjects/{accountingSubjectId}");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.Forbidden, problem.Status);
        Assert.Contains("must have power to view a chart of accounts", problem.Detail);
    }
}
