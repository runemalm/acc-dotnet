using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

public sealed class PostgresAccountStore : IAccountStore
{
    public AccountView? Find(Guid accountingSubjectId, string accountNumber) =>
        throw new NotSupportedException("Postgres account projection persistence has not been implemented yet.");

    public IReadOnlyCollection<AccountView> FindFor(Guid chartOfAccountsId) =>
        throw new NotSupportedException("Postgres account projection persistence has not been implemented yet.");

    public void Save(AccountView account) =>
        throw new NotSupportedException("Postgres account projection persistence has not been implemented yet.");

    public void Save(IReadOnlyCollection<AccountView> accounts) =>
        throw new NotSupportedException("Postgres account projection persistence has not been implemented yet.");
}
