using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Domain.Events;

namespace ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

public sealed class ChartOfAccountsProjection
{
    private readonly IChartOfAccountsStore charts;
    private readonly IAccountStore accounts;

    public ChartOfAccountsProjection(
        IChartOfAccountsStore charts,
        IAccountStore accounts)
    {
        this.charts = charts;
        this.accounts = accounts;
    }

    public void Project(ChartOfAccountsAdopted domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        charts.Save(new ChartOfAccountsView(
            domainEvent.ChartOfAccountsId,
            domainEvent.AccountingSubjectId,
            new AdoptedChartOfAccountsTemplateView(
                domainEvent.Template.Id,
                domainEvent.Template.Name),
            domainEvent.AdoptedAt));

        accounts.Save(domainEvent.Accounts
            .Select(account => new AccountView(
                domainEvent.ChartOfAccountsId,
                domainEvent.AccountingSubjectId,
                account.Number,
                account.Name,
                account.IsActive))
            .ToArray());
    }

    public void Project(AccountAdded domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        accounts.Save(new AccountView(
            domainEvent.ChartOfAccountsId,
            domainEvent.AccountingSubjectId,
            domainEvent.AccountNumber,
            domainEvent.AccountName,
            IsActive: true));
    }

    public void Project(AccountDeactivated domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var account = FindAccount(domainEvent.AccountingSubjectId, domainEvent.AccountNumber);
        accounts.Save(account with { IsActive = false });
    }

    public void Project(AccountReactivated domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var account = FindAccount(domainEvent.AccountingSubjectId, domainEvent.AccountNumber);
        accounts.Save(account with { IsActive = true });
    }

    private AccountView FindAccount(Guid accountingSubjectId, string accountNumber) =>
        accounts.Find(accountingSubjectId, accountNumber)
        ?? throw new InvalidOperationException(
            $"Account {accountNumber} could not be found in the chart of accounts projection.");
}
