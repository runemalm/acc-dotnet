using ACC.Bas.Tooling.Publications.Model;
using ACC.Bas.Tooling.Workbooks;

namespace ACC.Bas.Tooling.Publications.Parsing;

internal sealed class Bas2026WorkbookParser
{
    public IReadOnlyCollection<BasAccount> Parse(
        ExcelWorkbook workbook,
        string expectedVersion,
        string? expectedRevision)
    {
        var worksheet = workbook.Worksheets.SingleOrDefault(
                            candidate => candidate.Cell(1, 1)?.Text.StartsWith(
                                "KONTOPLAN BAS",
                                StringComparison.OrdinalIgnoreCase) == true)
                        ?? throw new InvalidOperationException(
                            "Workbook is not recognized as a full BAS chart of accounts.");

        var title = worksheet.Cell(1, 1)?.Text ?? string.Empty;
        var revision = worksheet.Cell(2, 1)?.Text ?? string.Empty;
        if (!title.Contains(expectedVersion, StringComparison.OrdinalIgnoreCase) ||
            expectedRevision is not null &&
            !revision.Contains(expectedRevision, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The BAS workbook version or revision does not match the expected source.");
        }

        var accounts = new List<BasAccount>();
        ParseColumns(worksheet, numberColumn: 1, nameColumn: 2, accounts);
        ParseColumns(worksheet, numberColumn: 3, nameColumn: 4, accounts);

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

            var parsed = BasAccountNumber.Parse(numberCell, allowK2Marker: true);
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
                parsed.HasK2Marker ? new[] { "K2" } : Array.Empty<string>()));
        }
    }
}
