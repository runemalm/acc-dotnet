namespace ACC.ChartOfAccounts.Application.UseCases.AddAccount;

public sealed record AddAccountCommand(
    Guid ActorUserId,
    Guid ChartOfAccountsId,
    string AccountNumber,
    string AccountName);
