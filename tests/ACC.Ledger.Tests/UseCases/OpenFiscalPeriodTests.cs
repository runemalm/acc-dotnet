using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Tests.TestKit;
using Xunit;

namespace ACC.Ledger.Tests.UseCases;

public sealed class OpenFiscalPeriodTests
{
    [Fact]
    public void GivenFiscalPeriod_WhenOpening_ThenFiscalPeriodOpened()
    {
        var context = new LedgerUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var startsOn = new DateOnly(2026, 1, 1);
        var endsOn = new DateOnly(2026, 12, 31);

        var result = context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(accountingSubjectId, startsOn, endsOn),
            DateTimeOffset.UtcNow);

        var fiscalPeriod = context.FindFiscalPeriodFor(accountingSubjectId, startsOn);

        Assert.NotEqual(Guid.Empty, result.FiscalPeriodId);
        Assert.NotNull(fiscalPeriod);
        Assert.Equal(result.FiscalPeriodId, fiscalPeriod.FiscalPeriodId);
        Assert.Equal(FiscalPeriodStatus.Open, fiscalPeriod.Status);
    }
}
