using ACC.Bas.Tooling.Publications.Parsing;
using ACC.Bas.Tooling.Workbooks;

namespace ACC.Bas.Tooling.ChartOfAccounts.Generation.Compilers;

internal sealed class BasK12018TemplateCompiler : IChartOfAccountsTemplateCompiler
{
    private const string PublicationVersion = "2018";
    private readonly ExcelWorkbookReader workbookReader;
    private readonly BasK1WorkbookParser parser;

    public BasK12018TemplateCompiler(
        ExcelWorkbookReader workbookReader,
        BasK1WorkbookParser parser)
    {
        this.workbookReader = workbookReader;
        this.parser = parser;
    }

    public string Name => "bas-k1-2018";

    public IReadOnlyCollection<TemplateAccount> Compile(
        TemplateRecipe recipe,
        IReadOnlyDictionary<string, string> inputs)
    {
        var path = RequireInput(inputs, "accountDefinitions");
        return parser.Parse(workbookReader.Read(path), PublicationVersion)
            .Select(account => new TemplateAccount(account.Number, account.Name))
            .ToArray();
    }

    private static string RequireInput(
        IReadOnlyDictionary<string, string> inputs,
        string name) =>
        inputs.GetValueOrDefault(name)
        ?? throw new InvalidOperationException($"The {name} recipe input is required.");
}
