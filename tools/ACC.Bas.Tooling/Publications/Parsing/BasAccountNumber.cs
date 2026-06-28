using System.Globalization;
using System.Text.RegularExpressions;
using ACC.Bas.Tooling.Publications.Model;
using ACC.Bas.Tooling.Workbooks;

namespace ACC.Bas.Tooling.Publications.Parsing;

internal static partial class BasAccountNumber
{
    public static ParsedAccountNumber? Parse(ExcelCell cell, bool allowK2Marker)
    {
        if (cell.Value is double numeric &&
            numeric is >= 1000 and <= 9999 &&
            numeric == Math.Truncate(numeric))
        {
            var number = numeric.ToString("0000", CultureInfo.InvariantCulture);
            return new ParsedAccountNumber(
                number,
                Kind(number),
                HasK2Marker: false);
        }

        var match = AccountNumberPattern().Match(cell.Text);
        if (!match.Success || (!allowK2Marker && cell.Text.Contains('#')))
        {
            return null;
        }

        var parsedNumber = match.Groups[1].Value;
        return new ParsedAccountNumber(
            parsedNumber,
            Kind(parsedNumber),
            cell.Text.Contains('#'));
    }

    private static BasAccountKind Kind(string number) =>
        number.EndsWith("00", StringComparison.Ordinal)
            ? BasAccountKind.Group
            : number.EndsWith('0')
                ? BasAccountKind.Main
                : BasAccountKind.Sub;

    [GeneratedRegex("^#?([0-9]{4})#?$", RegexOptions.CultureInvariant)]
    private static partial Regex AccountNumberPattern();
}

internal sealed record ParsedAccountNumber(
    string Number,
    BasAccountKind Kind,
    bool HasK2Marker);
