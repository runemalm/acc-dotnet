namespace ACC.Bas.Tooling.Publications.Model;

internal sealed record BasAccount(
    string Number,
    string Name,
    BasAccountKind Kind,
    IReadOnlyCollection<string> ExcludedAccountingFrameworks);

internal enum BasAccountKind
{
    Group,
    Main,
    Sub
}
