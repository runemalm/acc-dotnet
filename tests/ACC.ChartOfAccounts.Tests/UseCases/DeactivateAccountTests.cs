using ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;
using ACC.ChartOfAccounts.Domain.Invariants;
using ACC.ChartOfAccounts.Tests.TestKit;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.UseCases;

public sealed class DeactivateAccountTests
{
    [Fact]
    public void GivenUnrecognizedAccount_WhenDeactivating_ThenAccountMustBeRecognizedViolation()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var exception = Assert.Throws<AccountMustBeRecognizedByChartOfAccountsViolation>(() =>
            context.DeactivateAccount.Handle(
                new DeactivateAccountCommand(actorUserId, chartOfAccountsId, "9999"),
                DateTimeOffset.UtcNow));

        Assert.Contains("is not recognized by the chart of accounts", exception.Message);
    }

    [Fact]
    public void GivenActiveAccount_WhenDeactivating_ThenAccountDeactivated()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var result = context.DeactivateAccount.Handle(
            new DeactivateAccountCommand(actorUserId, chartOfAccountsId, "1000"),
            DateTimeOffset.UtcNow);

        var account = context.FindAccount(accountingSubjectId, "1000");
        Assert.Equal("1000", result.AccountNumber);
        Assert.NotNull(account);
        Assert.False(account.IsActive);
    }
}
