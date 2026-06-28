namespace ACC.Bas.Tooling.ChartOfAccounts.Generation;

internal sealed record ChartOfAccountsTemplateResource(
    string Id,
    string Name,
    IReadOnlyCollection<SourceArtifact> SourceArtifacts,
    IReadOnlyCollection<TemplateAccount> Accounts);

internal sealed record SourceArtifact(
    string FileName,
    string Sha256);

internal sealed record TemplateAccount(
    string Number,
    string Name);
