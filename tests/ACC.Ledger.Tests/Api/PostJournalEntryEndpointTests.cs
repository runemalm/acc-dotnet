using System.Net;
using System.Net.Http.Json;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Infrastructure.Endpoints;
using ACC.Ledger.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Ledger.Tests.Api;

public sealed class PostJournalEntryEndpointTests
{
    [Fact]
    public async Task PostJournalEntry_WithBalancedEntry_ReturnsCreated()
    {
        await using var context = await LedgerApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);

        context.OpenFiscalPeriod(
            accountingSubjectId,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));
        context.MakeAccountsActive(accountingSubjectId, "Cash", "Owner Equity");

        var response = await context.Client.PostAsJsonAsync(
            "/ledger/journal-entries",
            BalancedJournalEntry(accountingSubjectId, accountingDate));

        var result = await response.Content.ReadFromJsonAsync<PostJournalEntryResult>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.JournalEntryId);
        Assert.Equal(
            new Uri($"/ledger/journal-entries/{result.JournalEntryId}", UriKind.Relative),
            response.Headers.Location);
    }

    [Fact]
    public async Task PostJournalEntry_WithUnbalancedEntry_ReturnsUnprocessableEntity()
    {
        await using var context = await LedgerApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();

        context.OpenFiscalPeriod(
            accountingSubjectId,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));
        context.MakeAccountsActive(accountingSubjectId, "Cash", "Owner Equity");

        var response = await context.Client.PostAsJsonAsync(
            "/ledger/journal-entries",
            new PostJournalEntryRequest(
                accountingSubjectId,
                new DateOnly(2026, 6, 10),
                "Unbalanced entry",
                [
                    new PostJournalEntryRequestLine("Cash", 1000m, 0m),
                    new PostJournalEntryRequestLine("Owner Equity", 0m, 900m)
                ]));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, problem.Status);
        Assert.Contains("Journal entry must balance.", problem.Detail);
    }

    [Fact]
    public async Task PostJournalEntry_WithUnrecognizedAccount_ReturnsUnprocessableEntity()
    {
        await using var context = await LedgerApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);
        context.OpenFiscalPeriod(
            accountingSubjectId,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));
        context.MakeAccountsActive(accountingSubjectId, "Owner Equity");

        var response = await context.Client.PostAsJsonAsync(
            "/ledger/journal-entries",
            BalancedJournalEntry(accountingSubjectId, accountingDate));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("Account Cash must be recognized", problem.Detail);
    }

    private static PostJournalEntryRequest BalancedJournalEntry(
        Guid accountingSubjectId,
        DateOnly accountingDate) =>
        new(
            accountingSubjectId,
            accountingDate,
            "Initial capital contribution",
            [
                new PostJournalEntryRequestLine("Cash", 1000m, 0m),
                new PostJournalEntryRequestLine("Owner Equity", 0m, 1000m)
            ]);
}
