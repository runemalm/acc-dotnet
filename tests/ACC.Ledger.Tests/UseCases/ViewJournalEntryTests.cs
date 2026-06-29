using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Domain.Invariants;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Application.UseCases.ViewJournalEntry;
using ACC.Ledger.Tests.TestKit;
using Xunit;

namespace ACC.Ledger.Tests.UseCases;

public sealed class ViewJournalEntryTests
{
    [Fact]
    public void GivenActorWithViewJournalEntryPower_WhenViewing_ThenJournalEntryViewed()
    {
        var context = new LedgerUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var journalEntryId = PostJournalEntry(context, actorUserId, accountingSubjectId);

        var result = context.ViewJournalEntry.Handle(
            new ViewJournalEntryQuery(actorUserId, journalEntryId));

        Assert.NotNull(result);
        Assert.Equal(journalEntryId, result.JournalEntryId);
        Assert.Equal(accountingSubjectId, result.AccountingSubjectId);
    }

    [Fact]
    public void GivenActorWithoutViewJournalEntryPower_WhenViewing_ThenActorMustHavePowerViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var postingActorUserId = Guid.NewGuid();
        var viewingActorUserId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var journalEntryId = PostJournalEntry(context, postingActorUserId, accountingSubjectId);

        var exception = Assert.Throws<ActorMustHaveLedgerPowerViolation>(() =>
            context.ViewJournalEntry.Handle(
                new ViewJournalEntryQuery(viewingActorUserId, journalEntryId)));

        Assert.Contains("must have power to view a journal entry", exception.Message);
    }

    private static Guid PostJournalEntry(
        LedgerUseCaseTestContext context,
        Guid actorUserId,
        Guid accountingSubjectId)
    {
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

        return context.PostJournalEntry.Handle(
            new PostJournalEntryCommand(
                actorUserId,
                accountingSubjectId,
                accountingDate,
                "Initial capital contribution",
                [
                    new PostJournalEntryCommandLine("Cash", 1000m, 0m),
                    new PostJournalEntryCommandLine("Owner Equity", 0m, 1000m)
                ]),
            DateTimeOffset.UtcNow)
            .JournalEntryId;
    }
}
