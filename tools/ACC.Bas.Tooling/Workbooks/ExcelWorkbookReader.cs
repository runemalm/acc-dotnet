using ExcelDataReader;

namespace ACC.Bas.Tooling.Workbooks;

internal sealed class ExcelWorkbookReader
{
    public ExcelWorkbook Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var worksheets = new List<ExcelWorksheet>();

        do
        {
            var rows = new List<ExcelRow>();
            var rowNumber = 0;

            while (reader.Read())
            {
                rowNumber++;
                var cells = Enumerable.Range(0, reader.FieldCount)
                    .Select(index => new ExcelCell(rowNumber, index + 1, reader.GetValue(index)))
                    .ToArray();
                rows.Add(new ExcelRow(rowNumber, cells));
            }

            worksheets.Add(new ExcelWorksheet(reader.Name, rows));
        }
        while (reader.NextResult());

        return new ExcelWorkbook(worksheets);
    }
}
