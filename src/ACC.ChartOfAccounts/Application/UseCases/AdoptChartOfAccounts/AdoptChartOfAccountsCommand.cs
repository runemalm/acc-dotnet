namespace ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;

public sealed record AdoptChartOfAccountsCommand(
    Guid ActorUserId,
    Guid AccountingSubjectId,
    string TemplateId);
