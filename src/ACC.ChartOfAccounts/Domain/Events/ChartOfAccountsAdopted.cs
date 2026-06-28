using ACC.ChartOfAccounts.Domain.Aggregates;
using ACC.ChartOfAccounts.Domain.Templates;

namespace ACC.ChartOfAccounts.Domain.Events;

public sealed record ChartOfAccountsAdopted
{
    public ChartOfAccountsAdopted(
        Guid chartOfAccountsId,
        Guid accountingSubjectId,
        AdoptedChartOfAccountsTemplate template,
        IReadOnlyCollection<Account> accounts,
        Guid adoptedByUserId,
        DateTimeOffset adoptedAt)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(accounts);

        ChartOfAccountsId = chartOfAccountsId;
        AccountingSubjectId = accountingSubjectId;
        Template = template;
        Accounts = accounts.ToArray();
        AdoptedByUserId = adoptedByUserId;
        AdoptedAt = adoptedAt;
    }

    public Guid ChartOfAccountsId { get; }

    public Guid AccountingSubjectId { get; }

    public AdoptedChartOfAccountsTemplate Template { get; }

    public IReadOnlyCollection<Account> Accounts { get; }

    public Guid AdoptedByUserId { get; }

    public DateTimeOffset AdoptedAt { get; }
}
