using System.Globalization;
using ACC.Bas.Tooling.Publications.Model;
using ACC.Bas.Tooling.Publications.Parsing;
using ACC.Bas.Tooling.Workbooks;

namespace ACC.Bas.Tooling.ChartOfAccounts.Generation.Compilers;

internal sealed class BasK1Mini2018TemplateCompiler : IChartOfAccountsTemplateCompiler
{
    private const string PublicationVersion = "2018";
    private readonly ExcelWorkbookReader workbookReader;
    private readonly BasK1WorkbookParser accountParser;
    private readonly BasK1MiniWorkbookParser selectionParser;

    public BasK1Mini2018TemplateCompiler(
        ExcelWorkbookReader workbookReader,
        BasK1WorkbookParser accountParser,
        BasK1MiniWorkbookParser selectionParser)
    {
        this.workbookReader = workbookReader;
        this.accountParser = accountParser;
        this.selectionParser = selectionParser;
    }

    public string Name => "bas-k1-mini-2018";

    public IReadOnlyCollection<TemplateAccount> Compile(
        TemplateRecipe recipe,
        IReadOnlyDictionary<string, string> inputs)
    {
        var definitionsPath = RequireInput(inputs, "accountDefinitions");
        var selectionPath = RequireInput(inputs, "accountSelection");
        var definitions = accountParser.Parse(
                workbookReader.Read(definitionsPath),
                PublicationVersion)
            .ToDictionary(account => account.Number, StringComparer.Ordinal);
        var selections = selectionParser.Parse(
            workbookReader.Read(selectionPath),
            PublicationVersion);
        var selected = new Dictionary<string, BasAccount>(StringComparer.Ordinal);

        foreach (var selection in selections)
        {
            var matches = Select(definitions, selection);
            if (matches.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Selection {Describe(selection)} " +
                    $"at {selection.Worksheet}!{selection.Cell} selects no full K1 accounts.");
            }

            foreach (var account in matches)
            {
                selected.TryAdd(account.Number, account);
            }
        }

        return selected.Values
            .Select(account => new TemplateAccount(account.Number, account.Name))
            .ToArray();
    }

    private static BasAccount[] Select(
        IReadOnlyDictionary<string, BasAccount> definitions,
        BasAccountSelection selection) =>
        selection switch
        {
            BasExactAccountSelection exact => SelectExact(definitions, exact),
            BasMainAccountRangeSelection range => definitions.Values
                .Where(account => account.Kind == BasAccountKind.Main)
                .Where(account => IsWithin(account.Number, range))
                .ToArray(),
            _ => throw new InvalidOperationException(
                $"Selection type {selection.GetType().Name} is not recognized.")
        };

    private static BasAccount[] SelectExact(
        IReadOnlyDictionary<string, BasAccount> definitions,
        BasExactAccountSelection selection)
    {
        var number = selection.AccountNumber.ToString("0000", CultureInfo.InvariantCulture);
        if (!definitions.TryGetValue(number, out var account) || account.Kind == BasAccountKind.Sub)
        {
            return [];
        }

        return [account];
    }

    private static bool IsWithin(
        string accountNumber,
        BasMainAccountRangeSelection selection)
    {
        var number = int.Parse(accountNumber, CultureInfo.InvariantCulture);
        return number >= selection.FirstAccountNumber && number <= selection.LastAccountNumber;
    }

    private static string Describe(BasAccountSelection selection) =>
        selection switch
        {
            BasExactAccountSelection exact =>
                exact.AccountNumber.ToString("0000", CultureInfo.InvariantCulture),
            BasMainAccountRangeSelection range =>
                $"{range.FirstAccountNumber:0000}-{range.LastAccountNumber:0000}",
            _ => selection.GetType().Name
        };

    private static string RequireInput(
        IReadOnlyDictionary<string, string> inputs,
        string name) =>
        inputs.GetValueOrDefault(name)
        ?? throw new InvalidOperationException($"The {name} recipe input is required.");
}
