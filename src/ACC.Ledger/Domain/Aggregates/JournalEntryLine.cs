namespace ACC.Ledger.Domain.Aggregates;

public sealed record JournalEntryLine
{
    public JournalEntryLine(string account, decimal debit, decimal credit)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (string.IsNullOrWhiteSpace(account))
        {
            throw new ArgumentException("A journal entry line must name an account.", nameof(account));
        }

        if (debit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(debit), "Debit cannot be negative.");
        }

        if (credit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(credit), "Credit cannot be negative.");
        }

        if (debit == 0 && credit == 0)
        {
            throw new ArgumentException("A journal entry line must carry either a debit or a credit.");
        }

        if (debit > 0 && credit > 0)
        {
            throw new ArgumentException("A journal entry line cannot carry both a debit and a credit.");
        }

        Account = account;
        Debit = debit;
        Credit = credit;
    }

    public string Account { get; }

    public decimal Debit { get; }

    public decimal Credit { get; }
}
