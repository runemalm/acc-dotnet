namespace ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;

public sealed record ViewChartOfAccountsQuery(
    Guid ActorUserId,
    Guid AccountingSubjectId);
