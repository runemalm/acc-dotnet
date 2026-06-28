using ACC.Bas.Tooling.Publications.Model;
using ACC.Bas.Tooling.Workbooks;

namespace ACC.Bas.Tooling.Publications.Parsing;

internal sealed class BasK1WorkbookParser
{
    public IReadOnlyCollection<BasAccount> Parse(
        ExcelWorkbook workbook,
        string expectedVersion)
    {
        var worksheet = workbook.RequireWorksheet("Blad1");
        var title = worksheet.Cell(1, 2)?.Text ?? string.Empty;
        if (!title.Contains("K1", StringComparison.OrdinalIgnoreCase) ||
            !title.Contains("Kontoplan", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("Minimikontoplan", StringComparison.OrdinalIgnoreCase) ||
            !title.Contains(expectedVersion, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Workbook is not recognized as a full BAS K1 chart of accounts.");
        }

        var accounts = new List<BasAccount>();
        ParseColumns(worksheet, numberColumn: 2, nameColumn: 3, accounts);
        ParseColumns(worksheet, numberColumn: 5, nameColumn: 6, accounts);

        return accounts
            .OrderBy(account => account.Number, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ParseColumns(
        ExcelWorksheet worksheet,
        int numberColumn,
        int nameColumn,
        ICollection<BasAccount> accounts)
    {
        foreach (var row in worksheet.Rows)
        {
            var numberCell = worksheet.Cell(row.Number, numberColumn);
            if (numberCell is null)
            {
                continue;
            }

            var parsed = BasAccountNumber.Parse(numberCell, allowK2Marker: false);
            if (parsed is null)
            {
                continue;
            }

            var name = worksheet.Cell(row.Number, nameColumn)?.Text ?? string.Empty;
            if (name.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Account {parsed.Number} at {worksheet.Name}!{numberCell.Address} has no name.");
            }

            accounts.Add(new BasAccount(
                parsed.Number,
                name,
                parsed.Kind,
                Array.Empty<string>()));
        }
    }
}
