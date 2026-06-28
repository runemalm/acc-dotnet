using System.Net;
using System.Net.Http.Json;
using ACC.ChartOfAccounts.Application.UseCases.AddAccount;
using ACC.ChartOfAccounts.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.Api;

public sealed class AddAccountEndpointTests
{
    [Fact]
    public async Task AddAccount_WithUniqueNumber_ReturnsOk()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/chart-of-accounts/add-account",
            new AddAccountCommand(actorUserId, chartOfAccountsId, "3000", "Revenue"));

        var result = await response.Content.ReadFromJsonAsync<AddAccountResult>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("3000", result.AccountNumber);
    }

    [Fact]
    public async Task AddAccount_WithDuplicateNumber_ReturnsBadRequest()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/chart-of-accounts/add-account",
            new AddAccountCommand(actorUserId, chartOfAccountsId, "1000", "Another asset account"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("must be unique within the chart of accounts", problem.Detail);
    }
}
