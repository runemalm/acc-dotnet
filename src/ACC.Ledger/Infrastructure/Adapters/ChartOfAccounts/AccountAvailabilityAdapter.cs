using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.Ledger.Application.Ports.ChartOfAccounts;

namespace ACC.Ledger.Infrastructure.Adapters.ChartOfAccounts;

public sealed class AccountAvailabilityAdapter : IAccountAvailabilityPort
{
    private readonly IAccountStore accounts;

    public AccountAvailabilityAdapter(IAccountStore accounts)
    {
        this.accounts = accounts;
    }

    public PostingAccountAvailability GetAvailability(
        Guid accountingSubjectId,
        string accountNumber)
    {
        var account = accounts.Find(accountingSubjectId, accountNumber);

        return account switch
        {
            null => PostingAccountAvailability.Unrecognized,
            { IsActive: true } => PostingAccountAvailability.Active,
            _ => PostingAccountAvailability.Inactive
        };
    }
}
