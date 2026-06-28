namespace ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;

public sealed record DeactivateAccountCommand(
    Guid ActorUserId,
    Guid ChartOfAccountsId,
    string AccountNumber);
