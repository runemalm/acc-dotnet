namespace ACC.Bas.Tooling.Publications.Model;

internal abstract record BasAccountSelection(
    string Worksheet,
    string Cell);

internal sealed record BasExactAccountSelection(
    int AccountNumber,
    string Worksheet,
    string Cell) : BasAccountSelection(Worksheet, Cell);

internal sealed record BasMainAccountRangeSelection(
    int FirstAccountNumber,
    int LastAccountNumber,
    string Worksheet,
    string Cell) : BasAccountSelection(Worksheet, Cell);
