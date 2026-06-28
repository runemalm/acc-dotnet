using System.Net;
using System.Net.Http.Json;
using ACC.ChartOfAccounts.Application.UseCases.ReactivateAccount;
using ACC.ChartOfAccounts.Infrastructure.Endpoints;
using ACC.ChartOfAccounts.Tests.TestKit;
using ACC.Testing.Authentication;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.Api;

public sealed class ReactivateAccountEndpointTests
{
    [Fact]
    public async Task ReactivateAccount_WithInactiveAccount_ReturnsOk()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);
        context.DeactivateAccount(chartOfAccountsId, actorUserId, "1000");
        context.Client.AuthenticateAs(actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/chart-of-accounts/reactivate-account",
            new ChangeAccountAvailabilityRequest(chartOfAccountsId, "1000"));

        var result = await response.Content.ReadFromJsonAsync<ReactivateAccountResult>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("1000", result.AccountNumber);
    }

    [Fact]
    public async Task ReactivateAccount_WithActiveAccount_ReturnsConflict()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);
        context.Client.AuthenticateAs(actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/chart-of-accounts/reactivate-account",
            new ChangeAccountAvailabilityRequest(chartOfAccountsId, "1000"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("is already active", problem.Detail);
    }
}
