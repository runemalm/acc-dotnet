using System.Text;
using ACC.Bas.Tooling.ChartOfAccounts.Generation;
using ACC.Bas.Tooling.ChartOfAccounts.Generation.Compilers;
using ACC.Bas.Tooling.Publications.Parsing;
using ACC.Bas.Tooling.Workbooks;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

if (args.Length != 2 || !string.Equals(args[0], "generate", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine(
        "Usage: ACC.Bas.Tooling generate <recipe.json>");
    return 1;
}

var workbookReader = new ExcelWorkbookReader();
var basK1Parser = new BasK1WorkbookParser();
var generator = new BasChartOfAccountsTemplateGenerator(
    new IChartOfAccountsTemplateCompiler[]
    {
        new BasK12018TemplateCompiler(workbookReader, basK1Parser),
        new BasK1Mini2018TemplateCompiler(
            workbookReader,
            basK1Parser,
            new BasK1MiniWorkbookParser())
    },
    new TemplateResourceWriter());

var result = await generator.Generate(Path.GetFullPath(args[1]));

Console.WriteLine($"Generated {result.AccountCount} accounts.");
Console.WriteLine($"Wrote {result.OutputPath}.");

return 0;
