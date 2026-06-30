using ACC.BuildingBlocks.Authorization;
using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Domain.Invariants;
using ACC.Ledger.Tests.TestKit;
using Xunit;

namespace ACC.Ledger.Tests.UseCases;

public sealed class PostJournalEntryTests
{
    [Fact]
    public void GivenBalancedJournalEntryAndOpenFiscalPeriod_WhenPosting_ThenJournalEntryPosted()
    {
        var context = new LedgerUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);
        context.AllowAllLedgerActs(actorUserId, accountingSubjectId);

        context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                actorUserId,
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);
        context.MakeAccountsActive(accountingSubjectId, "Cash", "Owner Equity");

        var result = context.PostJournalEntry.Handle(
            BalancedJournalEntry(actorUserId, accountingSubjectId, accountingDate),
            DateTimeOffset.UtcNow);

        var journalEntry = context.FindJournalEntry(result.JournalEntryId);

        Assert.NotEqual(Guid.Empty, result.JournalEntryId);
        Assert.NotNull(journalEntry);
        Assert.Equal(accountingSubjectId, journalEntry.AccountingSubjectId);
        Assert.Equal(accountingDate, journalEntry.AccountingDate);
    }

    [Fact]
    public void GivenNoFiscalPeriodForAccountingDate_WhenPosting_ThenPostingMustOccurInOpenPeriodViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        context.AllowPostingJournalEntry(actorUserId, accountingSubjectId);
        var command = BalancedJournalEntry(
            actorUserId,
            accountingSubjectId,
            new DateOnly(2026, 6, 10));

        var exception = Assert.Throws<PostingMustOccurInOpenPeriodViolation>(() =>
            context.PostJournalEntry.Handle(command, DateTimeOffset.UtcNow));

        Assert.Contains("no fiscal period contains that date", exception.Message);
    }

    [Fact]
    public void GivenUnbalancedJournalEntry_WhenPosting_ThenJournalEntryMustBalanceViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);
        context.AllowAllLedgerActs(actorUserId, accountingSubjectId);

        context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                actorUserId,
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);
        context.MakeAccountsActive(accountingSubjectId, "Cash", "Owner Equity");

        var command = new PostJournalEntryCommand(
            actorUserId,
            accountingSubjectId,
            accountingDate,
            "Unbalanced entry",
            [
                new PostJournalEntryCommandLine("Cash", 1000m, 0m),
                new PostJournalEntryCommandLine("Owner Equity", 0m, 900m)
            ]);

        var exception = Assert.Throws<JournalEntryMustBalanceViolation>(() =>
            context.PostJournalEntry.Handle(command, DateTimeOffset.UtcNow));

        Assert.Contains("Journal entry must balance.", exception.Message);
    }

    [Fact]
    public void GivenClosedFiscalPeriod_WhenPosting_ThenPostingMustOccurInOpenPeriodViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);
        context.AllowAllLedgerActs(actorUserId, accountingSubjectId);

        var opened = context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                actorUserId,
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);

        context.CloseFiscalPeriod.Handle(
            new CloseFiscalPeriodCommand(actorUserId, opened.FiscalPeriodId),
            DateTimeOffset.UtcNow);
        context.MakeAccountsActive(accountingSubjectId, "Cash", "Owner Equity");

        var exception = Assert.Throws<PostingMustOccurInOpenPeriodViolation>(() =>
            context.PostJournalEntry.Handle(
                BalancedJournalEntry(actorUserId, accountingSubjectId, accountingDate),
                DateTimeOffset.UtcNow));

        Assert.Contains("fiscal period is not open", exception.Message);
    }

    [Fact]
    public void GivenUnrecognizedAccount_WhenPosting_ThenPostingAccountMustBeRecognizedViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);
        context.AllowAllLedgerActs(actorUserId, accountingSubjectId);
        context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                actorUserId,
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);
        context.MakeAccountsActive(accountingSubjectId, "Owner Equity");

        var exception = Assert.Throws<PostingAccountMustBeRecognizedViolation>(() =>
            context.PostJournalEntry.Handle(
                BalancedJournalEntry(actorUserId, accountingSubjectId, accountingDate),
                DateTimeOffset.UtcNow));

        Assert.Contains("Account Cash must be recognized", exception.Message);
    }

    [Fact]
    public void GivenInactiveAccount_WhenPosting_ThenPostingAccountMustBeActiveViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);
        context.AllowAllLedgerActs(actorUserId, accountingSubjectId);
        context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                actorUserId,
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);
        context.MakeAccountInactive(accountingSubjectId, "Cash");
        context.MakeAccountsActive(accountingSubjectId, "Owner Equity");

        var exception = Assert.Throws<PostingAccountMustBeActiveViolation>(() =>
            context.PostJournalEntry.Handle(
                BalancedJournalEntry(actorUserId, accountingSubjectId, accountingDate),
                DateTimeOffset.UtcNow));

        Assert.Contains("Account Cash must be active", exception.Message);
    }

    [Fact]
    public void GivenActorWithoutPostJournalEntryPower_WhenPosting_ThenAuthorizationDenied()
    {
        var context = new LedgerUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var accountingDate = new DateOnly(2026, 6, 10);
        context.AllowOpeningFiscalPeriod(actorUserId, accountingSubjectId);
        context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                actorUserId,
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);
        context.MakeAccountsActive(accountingSubjectId, "Cash", "Owner Equity");

        var exception = Assert.Throws<AuthorizationDeniedException>(() =>
            context.PostJournalEntry.Handle(
                BalancedJournalEntry(actorUserId, accountingSubjectId, accountingDate),
                DateTimeOffset.UtcNow));

        Assert.Contains("must have power to post a journal entry", exception.Message);
    }

    private static PostJournalEntryCommand BalancedJournalEntry(
        Guid actorUserId,
        Guid accountingSubjectId,
        DateOnly accountingDate) =>
        new(
            actorUserId,
            accountingSubjectId,
            accountingDate,
            "Initial capital contribution",
            [
                new PostJournalEntryCommandLine("Cash", 1000m, 0m),
                new PostJournalEntryCommandLine("Owner Equity", 0m, 1000m)
            ]);
}
