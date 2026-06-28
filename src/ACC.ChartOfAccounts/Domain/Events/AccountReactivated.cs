namespace ACC.ChartOfAccounts.Domain.Events;

public sealed record AccountReactivated(
    Guid ChartOfAccountsId,
    Guid AccountingSubjectId,
    string AccountNumber,
    Guid ReactivatedByUserId,
    DateTimeOffset ReactivatedAt);
