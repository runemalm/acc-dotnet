using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Tests.TestKit;
using Xunit;

namespace ACC.Ledger.Tests.UseCases;

public sealed class PostJournalEntryTests
{
    [Fact]
    public void GivenBalancedJournalEntryAndOpenFiscalPeriod_WhenPosting_ThenJournalEntryPosted()
    {
        var context = new LedgerUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);

        context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);

        var result = context.PostJournalEntry.Handle(
            BalancedJournalEntry(accountingSubjectId, accountingDate),
            DateTimeOffset.UtcNow);

        var journalEntry = context.FindJournalEntry(result.JournalEntryId);

        Assert.NotEqual(Guid.Empty, result.JournalEntryId);
        Assert.NotNull(journalEntry);
        Assert.Equal(accountingDate, journalEntry.AccountingDate);
    }

    [Fact]
    public void GivenNoFiscalPeriodForAccountingDate_WhenPosting_ThenPostingMustOccurInOpenPeriodViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var command = BalancedJournalEntry(Guid.NewGuid(), new DateOnly(2026, 6, 10));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.PostJournalEntry.Handle(command, DateTimeOffset.UtcNow));

        Assert.Contains("No fiscal period contains accounting date", exception.Message);
    }

    [Fact]
    public void GivenUnbalancedJournalEntry_WhenPosting_ThenJournalEntryMustBalanceViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);

        context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);

        var command = new PostJournalEntryCommand(
            accountingSubjectId,
            accountingDate,
            "Unbalanced entry",
            [
                new PostJournalEntryCommandLine("Cash", 1000m, 0m),
                new PostJournalEntryCommandLine("Owner Equity", 0m, 900m)
            ]);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.PostJournalEntry.Handle(command, DateTimeOffset.UtcNow));

        Assert.Contains("Journal entry must balance.", exception.Message);
    }

    [Fact]
    public void GivenClosedFiscalPeriod_WhenPosting_ThenPostingMustOccurInOpenPeriodViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);

        var opened = context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);

        context.CloseFiscalPeriod.Handle(
            new CloseFiscalPeriodCommand(opened.FiscalPeriodId),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.PostJournalEntry.Handle(
                BalancedJournalEntry(accountingSubjectId, accountingDate),
                DateTimeOffset.UtcNow));

        Assert.Contains("fiscal period is not open", exception.Message);
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
