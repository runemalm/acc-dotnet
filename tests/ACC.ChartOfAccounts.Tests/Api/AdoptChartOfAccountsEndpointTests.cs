using System.Net;
using System.Net.Http.Json;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;
using ACC.ChartOfAccounts.Infrastructure.Endpoints;
using ACC.ChartOfAccounts.Tests.TestKit;
using ACC.Testing.Authentication;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.Api;

public sealed class AdoptChartOfAccountsEndpointTests
{
    [Fact]
    public async Task AdoptChartOfAccounts_WithRecognizedTemplate_ReturnsCreated()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        context.AddTemplate();
        context.RecognizeAccountingSubject(accountingSubjectId);
        context.AllowAdoption(actorUserId, accountingSubjectId);
        context.Client.AuthenticateAs(actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/chart-of-accounts/adopt",
            new AdoptChartOfAccountsRequest(
                accountingSubjectId,
                "test-template"));

        var result = await response.Content.ReadFromJsonAsync<AdoptChartOfAccountsResult>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.ChartOfAccountsId);
        Assert.Equal(
            new Uri($"/chart-of-accounts/accounting-subjects/{accountingSubjectId}", UriKind.Relative),
            response.Headers.Location);
    }

    [Fact]
    public async Task AdoptChartOfAccounts_WithUnknownTemplate_ReturnsNotFound()
    {
        await using var context = await ChartOfAccountsApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        context.RecognizeAccountingSubject(accountingSubjectId);
        context.AllowAdoption(actorUserId, accountingSubjectId);
        context.Client.AuthenticateAs(actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/chart-of-accounts/adopt",
            new AdoptChartOfAccountsRequest(
                accountingSubjectId,
                "unknown-template"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("is not recognized", problem.Detail);
    }
}
