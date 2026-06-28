namespace ACC.ChartOfAccounts.Domain.Aggregates;

public sealed record Account
{
    public Account(string number, string name, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("An account must have a number.", nameof(number));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("An account must have a name.", nameof(name));
        }

        Number = number;
        Name = name;
        IsActive = isActive;
    }

    public string Number { get; }

    public string Name { get; }

    public bool IsActive { get; init; }

    public Account Deactivate() => this with { IsActive = false };

    public Account Reactivate() => this with { IsActive = true };
}
