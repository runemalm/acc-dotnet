using System.Text;

namespace ACC.Bas.Tooling.Workbooks;

internal sealed record ExcelWorkbook(IReadOnlyCollection<ExcelWorksheet> Worksheets)
{
    public ExcelWorksheet RequireWorksheet(string name) =>
        Worksheets.SingleOrDefault(
            worksheet => string.Equals(worksheet.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException($"Workbook does not contain worksheet '{name}'.");
}

internal sealed record ExcelWorksheet(
    string Name,
    IReadOnlyCollection<ExcelRow> Rows)
{
    public ExcelCell? Cell(int rowNumber, int columnNumber) =>
        Rows.SingleOrDefault(row => row.Number == rowNumber)?.Cells
            .SingleOrDefault(cell => cell.ColumnNumber == columnNumber);
}

internal sealed record ExcelRow(
    int Number,
    IReadOnlyCollection<ExcelCell> Cells);

internal sealed record ExcelCell(
    int RowNumber,
    int ColumnNumber,
    object? Value)
{
    public string Text => (Value?.ToString() ?? string.Empty)
        .Trim()
        .Normalize(NormalizationForm.FormC);

    public string Address => $"{ColumnName(ColumnNumber)}{RowNumber}";

    private static string ColumnName(int columnNumber)
    {
        var name = string.Empty;
        var value = columnNumber;

        while (value > 0)
        {
            value--;
            name = (char)('A' + value % 26) + name;
            value /= 26;
        }

        return name;
    }
}
