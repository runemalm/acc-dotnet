namespace ACC.Bas.Tooling.ChartOfAccounts.Generation;

internal interface IChartOfAccountsTemplateCompiler
{
    string Name { get; }

    IReadOnlyCollection<TemplateAccount> Compile(
        TemplateRecipe recipe,
        IReadOnlyDictionary<string, string> inputs);
}
