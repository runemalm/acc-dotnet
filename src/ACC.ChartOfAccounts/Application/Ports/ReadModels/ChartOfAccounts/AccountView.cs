namespace ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

public sealed record AccountView(
    Guid ChartOfAccountsId,
    Guid AccountingSubjectId,
    string Number,
    string Name,
    bool IsActive);
