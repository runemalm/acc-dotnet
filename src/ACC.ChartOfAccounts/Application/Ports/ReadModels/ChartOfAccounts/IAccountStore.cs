namespace ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

public interface IAccountStore
{
    AccountView? Find(Guid accountingSubjectId, string accountNumber);

    IReadOnlyCollection<AccountView> FindFor(Guid chartOfAccountsId);

    void Save(AccountView account);

    void Save(IReadOnlyCollection<AccountView> accounts);
}
