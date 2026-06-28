using System.Net;
using System.Net.Http.Json;
using ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;
using ACC.ChartOfAccounts.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.Api;

public sealed class DeactivateAccountEndpointTests
{
    [Fact]
    public async Task DeactivateAccount_WithActiveAccount_ReturnsOk()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/chart-of-accounts/deactivate-account",
            new DeactivateAccountCommand(actorUserId, chartOfAccountsId, "1000"));

        var result = await response.Content.ReadFromJsonAsync<DeactivateAccountResult>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("1000", result.AccountNumber);
    }

    [Fact]
    public async Task DeactivateAccount_WithUnknownAccount_ReturnsBadRequest()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/chart-of-accounts/deactivate-account",
            new DeactivateAccountCommand(actorUserId, chartOfAccountsId, "9999"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("is not recognized by the chart of accounts", problem.Detail);
    }
}
