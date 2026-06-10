using System.Net;
using System.Net.Http.Json;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Application.UseCases.ViewJournalEntry;
using ACC.Ledger.Tests.TestKit;
using Xunit;

namespace ACC.Ledger.Tests.Api;

public sealed class ViewJournalEntryEndpointTests
{
    [Fact]
    public async Task ViewJournalEntry_WithExistingJournalEntry_ReturnsOk()
    {
        await using var context = await LedgerApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);

        context.OpenFiscalPeriod(
            accountingSubjectId,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));

        var posted = await context.Client.PostAsJsonAsync(
            "/ledger/journal-entries",
            BalancedJournalEntry(accountingSubjectId, accountingDate));

        var postedResult = await posted.Content.ReadFromJsonAsync<PostJournalEntryResult>();

        var response = await context.Client.GetAsync($"/ledger/journal-entries/{postedResult!.JournalEntryId}");

        var journalEntry = await response.Content.ReadFromJsonAsync<ViewJournalEntryResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(journalEntry);
        Assert.Equal(postedResult.JournalEntryId, journalEntry.JournalEntryId);
        Assert.Equal(accountingDate, journalEntry.AccountingDate);
        Assert.Equal("Initial capital contribution", journalEntry.Description);
        Assert.Equal(2, journalEntry.Lines.Count);
    }

    [Fact]
    public async Task ViewJournalEntry_WithUnknownJournalEntry_ReturnsNotFound()
    {
        await using var context = await LedgerApiTestContext.Create();

        var response = await context.Client.GetAsync($"/ledger/journal-entries/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static PostJournalEntryCommand BalancedJournalEntry(
        Guid accountingSubjectId,
        DateOnly accountingDate) =>
        new(
            accountingSubjectId,
            accountingDate,
            "Initial capital contribution",
            [
                new PostJournalEntryCommandLine("Cash", 1000m, 0m),
                new PostJournalEntryCommandLine("Owner Equity", 0m, 1000m)
            ]);
}
