namespace ACC.ChartOfAccounts.Application.UseCases.ReactivateAccount;

public sealed record ReactivateAccountCommand(
    Guid ActorUserId,
    Guid ChartOfAccountsId,
    string AccountNumber);
