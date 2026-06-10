using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Tests.TestKit;
using Xunit;

namespace ACC.Ledger.Tests.UseCases;

public sealed class CloseFiscalPeriodTests
{
    [Fact]
    public void GivenOpenFiscalPeriod_WhenClosing_ThenFiscalPeriodClosed()
    {
        var context = new LedgerUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var opened = context.OpenFiscalPeriod.Handle(
            new OpenFiscalPeriodCommand(
                accountingSubjectId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)),
            DateTimeOffset.UtcNow);

        var closed = context.CloseFiscalPeriod.Handle(
            new CloseFiscalPeriodCommand(opened.FiscalPeriodId),
            DateTimeOffset.UtcNow);

        var fiscalPeriod = context.FindFiscalPeriod(opened.FiscalPeriodId);

        Assert.Equal(opened.FiscalPeriodId, closed.FiscalPeriodId);
        Assert.NotNull(fiscalPeriod);
        Assert.Equal(FiscalPeriodStatus.Closed, fiscalPeriod.Status);
    }

    [Fact]
    public void GivenClosedFiscalPeriod_WhenClosing_ThenFiscalPeriodMustBeOpenToCloseViolation()
    {
        var context = new LedgerUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
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
            context.CloseFiscalPeriod.Handle(
                new CloseFiscalPeriodCommand(opened.FiscalPeriodId),
                DateTimeOffset.UtcNow));

        Assert.Equal("A fiscal period must be open before it can be closed.", exception.Message);
    }
}
