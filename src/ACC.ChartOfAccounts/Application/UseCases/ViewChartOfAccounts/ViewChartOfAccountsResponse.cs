namespace ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;

public sealed record ViewChartOfAccountsResponse(
    Guid ChartOfAccountsId,
    Guid AccountingSubjectId,
    AdoptedChartOfAccountsTemplateResponse Template,
    DateTimeOffset AdoptedAt,
    IReadOnlyCollection<ViewAccountResponse> Accounts);

public sealed record AdoptedChartOfAccountsTemplateResponse(
    string Id,
    string Name);

public sealed record ViewAccountResponse(
    string Number,
    string Name,
    bool IsActive);
