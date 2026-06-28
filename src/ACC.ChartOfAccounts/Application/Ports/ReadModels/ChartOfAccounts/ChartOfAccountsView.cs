namespace ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;

public sealed record ChartOfAccountsView(
    Guid ChartOfAccountsId,
    Guid AccountingSubjectId,
    AdoptedChartOfAccountsTemplateView Template,
    DateTimeOffset AdoptedAt);

public sealed record AdoptedChartOfAccountsTemplateView(
    string Id,
    string Name);
