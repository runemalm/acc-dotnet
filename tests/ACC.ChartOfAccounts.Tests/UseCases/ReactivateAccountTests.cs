using ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ReactivateAccount;
using ACC.ChartOfAccounts.Tests.TestKit;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.UseCases;

public sealed class ReactivateAccountTests
{
    [Fact]
    public void GivenInactiveAccount_WhenReactivating_ThenAccountReactivated()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);
        context.DeactivateAccount.Handle(
            new DeactivateAccountCommand(actorUserId, chartOfAccountsId, "1000"),
            DateTimeOffset.UtcNow);

        var result = context.ReactivateAccount.Handle(
            new ReactivateAccountCommand(actorUserId, chartOfAccountsId, "1000"),
            DateTimeOffset.UtcNow);

        var account = context.FindAccount(accountingSubjectId, "1000");
        Assert.Equal("1000", result.AccountNumber);
        Assert.NotNull(account);
        Assert.True(account.IsActive);
    }
}
