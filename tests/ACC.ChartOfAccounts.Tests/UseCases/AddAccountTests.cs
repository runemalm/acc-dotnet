using ACC.BuildingBlocks.Failures;
using ACC.ChartOfAccounts.Application.UseCases.AddAccount;
using ACC.ChartOfAccounts.Tests.TestKit;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.UseCases;

public sealed class AddAccountTests
{
    [Fact]
    public void GivenUnusedAccountNumber_WhenAddingAccount_ThenAccountAdded()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var result = context.AddAccount.Handle(
            new AddAccountCommand(
                actorUserId,
                chartOfAccountsId,
                "3000",
                "Revenue"),
            DateTimeOffset.UtcNow);

        var account = context.FindAccount(accountingSubjectId, "3000");
        Assert.Equal("3000", result.AccountNumber);
        Assert.NotNull(account);
        Assert.Equal("Revenue", account.Name);
        Assert.True(account.IsActive);
    }

    [Fact]
    public void GivenExistingAccountNumber_WhenAddingAccount_ThenAccountNumberMustBeUniqueViolation()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var chartOfAccountsId = context.AdoptChart(accountingSubjectId, actorUserId);

        var exception = Assert.Throws<StateConflictException>(() =>
            context.AddAccount.Handle(
                new AddAccountCommand(
                    actorUserId,
                    chartOfAccountsId,
                    "1000",
                    "Another asset account"),
                DateTimeOffset.UtcNow));

        Assert.Contains("must be unique within the chart of accounts", exception.Message);
    }
}
