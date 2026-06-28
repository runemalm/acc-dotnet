namespace ACC.Bas.Tooling.ChartOfAccounts.Generation;

internal sealed record TemplateRecipe(
    string Id,
    string Name,
    string Compiler,
    IReadOnlyDictionary<string, string> Inputs,
    string Output);
