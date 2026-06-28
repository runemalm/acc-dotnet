namespace ACC.ChartOfAccounts.Domain.Events;

public sealed record AccountDeactivated(
    Guid ChartOfAccountsId,
    Guid AccountingSubjectId,
    string AccountNumber,
    Guid DeactivatedByUserId,
    DateTimeOffset DeactivatedAt);
