namespace ACC.ChartOfAccounts.Application.Ports.Templates;

public sealed record ChartOfAccountsTemplate(
    string Id,
    string Name,
    IReadOnlyCollection<TemplateAccount> Accounts);

public sealed record TemplateAccount(
    string Number,
    string Name);
