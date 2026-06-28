namespace ACC.ChartOfAccounts.Domain.Events;

public sealed record AccountAdded(
    Guid ChartOfAccountsId,
    Guid AccountingSubjectId,
    string AccountNumber,
    string AccountName,
    Guid AddedByUserId,
    DateTimeOffset AddedAt);
