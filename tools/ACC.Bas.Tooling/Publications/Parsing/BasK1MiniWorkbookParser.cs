using System.Globalization;
using System.Text.RegularExpressions;
using ACC.Bas.Tooling.Publications.Model;
using ACC.Bas.Tooling.Workbooks;

namespace ACC.Bas.Tooling.Publications.Parsing;

internal sealed partial class BasK1MiniWorkbookParser
{
    public IReadOnlyCollection<BasAccountSelection> Parse(
        ExcelWorkbook workbook,
        string expectedVersion)
    {
        var worksheet = workbook.RequireWorksheet("Blad1");
        var title = worksheet.Cell(1, 2)?.Text ?? string.Empty;
        if (!title.Contains("K1", StringComparison.OrdinalIgnoreCase) ||
            !title.Contains("Minimikontoplan", StringComparison.OrdinalIgnoreCase) ||
            !title.Contains(expectedVersion, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Workbook is not recognized as a BAS K1 Mini chart of accounts.");
        }

        var selections = new List<BasAccountSelection>();
        foreach (var row in worksheet.Rows)
        {
            var cell = worksheet.Cell(row.Number, 4);
            if (cell is null || cell.Text.Length == 0)
            {
                continue;
            }

            foreach (var expression in cell.Text.Split(',', StringSplitOptions.TrimEntries))
            {
                selections.Add(ParseSelection(expression, worksheet.Name, cell.Address));
            }
        }

        return selections;
    }

    private static BasAccountSelection ParseSelection(
        string expression,
        string worksheet,
        string cell)
    {
        if (SingleAccountPattern().IsMatch(expression))
        {
            var number = int.Parse(expression, CultureInfo.InvariantCulture);
            return new BasExactAccountSelection(number, worksheet, cell);
        }

        var range = AccountRangePattern().Match(expression);
        if (!range.Success)
        {
            throw new InvalidOperationException(
                $"Account selection '{expression}' at {worksheet}!{cell} is not recognized.");
        }

        var first = int.Parse(range.Groups[1].Value, CultureInfo.InvariantCulture);
        var last = int.Parse(range.Groups[2].Value, CultureInfo.InvariantCulture);
        if (first > last)
        {
            throw new InvalidOperationException(
                $"Account range '{expression}' at {worksheet}!{cell} is reversed.");
        }

        return new BasMainAccountRangeSelection(first, last, worksheet, cell);
    }

    [GeneratedRegex("^[0-9]{4}$", RegexOptions.CultureInvariant)]
    private static partial Regex SingleAccountPattern();

    [GeneratedRegex("^([0-9]{4})\\s*[-–]\\s*([0-9]{4})$", RegexOptions.CultureInvariant)]
    private static partial Regex AccountRangePattern();
}
