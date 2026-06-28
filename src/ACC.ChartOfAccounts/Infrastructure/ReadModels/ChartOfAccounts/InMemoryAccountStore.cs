using System.Collections.Concurrent;
using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

public sealed class InMemoryAccountStore : IAccountStore
{
    private readonly ConcurrentDictionary<(Guid ChartOfAccountsId, string Number), AccountView> accounts = new();

    public AccountView? Find(Guid accountingSubjectId, string accountNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);

        return accounts.Values.SingleOrDefault(account =>
            account.AccountingSubjectId == accountingSubjectId &&
            account.Number == accountNumber);
    }

    public IReadOnlyCollection<AccountView> FindFor(Guid chartOfAccountsId) =>
        accounts.Values
            .Where(account => account.ChartOfAccountsId == chartOfAccountsId)
            .OrderBy(account => account.Number, StringComparer.Ordinal)
            .ToArray();

    public void Save(AccountView account)
    {
        ArgumentNullException.ThrowIfNull(account);
        accounts[(account.ChartOfAccountsId, account.Number)] = account;
    }

    public void Save(IReadOnlyCollection<AccountView> accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts);

        foreach (var account in accounts)
        {
            Save(account);
        }
    }
}
