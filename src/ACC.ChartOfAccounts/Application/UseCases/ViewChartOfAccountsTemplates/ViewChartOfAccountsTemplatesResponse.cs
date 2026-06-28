namespace ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccountsTemplates;

public sealed record ViewChartOfAccountsTemplatesResponse(
    IReadOnlyCollection<ChartOfAccountsTemplateResponse> Templates);

public sealed record ChartOfAccountsTemplateResponse(
    string TemplateId,
    string Name,
    int AccountCount);
