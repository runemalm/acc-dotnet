using ACC.BuildingBlocks.EventSourcing;
using ACC.ChartOfAccounts.Domain.Events;
using ACC.ChartOfAccounts.Domain.Invariants;
using ACC.ChartOfAccounts.Domain.Templates;

namespace ACC.ChartOfAccounts.Domain.Aggregates;

public sealed class ChartOfAccounts : EventSourcedAggregate
{
    private readonly Dictionary<string, Account> accounts = new(StringComparer.Ordinal);

    private ChartOfAccounts()
    {
    }

    public Guid Id { get; private set; }

    public Guid AccountingSubjectId { get; private set; }

    public AdoptedChartOfAccountsTemplate AdoptedTemplate { get; private set; } = null!;

    public DateTimeOffset AdoptedAt { get; private set; }

    public IReadOnlyCollection<Account> Accounts => accounts.Values.ToArray();

    public void AddAccount(
        string accountNumber,
        string accountName,
        Guid addedByUserId,
        DateTimeOffset addedAt)
    {
        EnsureActor(addedByUserId);

        var account = new Account(accountNumber, accountName);
        AccountNumberMustBeUniqueWithinChartOfAccounts.Ensure(Accounts, account.Number);

        Raise(new AccountAdded(
            Id,
            AccountingSubjectId,
            account.Number,
            account.Name,
            addedByUserId,
            addedAt));
    }

    public void DeactivateAccount(
        string accountNumber,
        Guid deactivatedByUserId,
        DateTimeOffset deactivatedAt)
    {
        EnsureActor(deactivatedByUserId);
        var account = FindAccount(accountNumber);

        AccountMustBeActiveToDeactivate.Ensure(account);

        Raise(new AccountDeactivated(
            Id,
            AccountingSubjectId,
            account.Number,
            deactivatedByUserId,
            deactivatedAt));
    }

    public void ReactivateAccount(
        string accountNumber,
        Guid reactivatedByUserId,
        DateTimeOffset reactivatedAt)
    {
        EnsureActor(reactivatedByUserId);
        var account = FindAccount(accountNumber);

        AccountMustBeInactiveToReactivate.Ensure(account);

        Raise(new AccountReactivated(
            Id,
            AccountingSubjectId,
            account.Number,
            reactivatedByUserId,
            reactivatedAt));
    }

    public static ChartOfAccounts Adopt(
        Guid id,
        Guid accountingSubjectId,
        AdoptedChartOfAccountsTemplate template,
        IReadOnlyCollection<Account> accounts,
        Guid adoptedByUserId,
        DateTimeOffset adoptedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A chart of accounts must have an identity.", nameof(id));
        }

        if (accountingSubjectId == Guid.Empty)
        {
            throw new ArgumentException(
                "A chart of accounts must belong to an accounting subject.",
                nameof(accountingSubjectId));
        }

        EnsureActor(adoptedByUserId);
        EnsureTemplate(template);
        ArgumentNullException.ThrowIfNull(accounts);
        AccountNumberMustBeUniqueWithinChartOfAccounts.Ensure(accounts);

        var chartOfAccounts = new ChartOfAccounts();
        chartOfAccounts.Raise(new ChartOfAccountsAdopted(
            id,
            accountingSubjectId,
            template,
            accounts,
            adoptedByUserId,
            adoptedAt));

        return chartOfAccounts;
    }

    public static ChartOfAccounts Rehydrate(IEnumerable<object> events)
    {
        var chartOfAccounts = new ChartOfAccounts();
        chartOfAccounts.LoadFromHistory(events);

        return chartOfAccounts;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ChartOfAccountsAdopted adopted:
                Apply(adopted);
                break;
            case AccountAdded added:
                Apply(added);
                break;
            case AccountDeactivated deactivated:
                Apply(deactivated);
                break;
            case AccountReactivated reactivated:
                Apply(reactivated);
                break;
        }
    }

    private static void EnsureActor(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("A chart of accounts act must identify the acting user.", nameof(actorUserId));
        }
    }

    private Account FindAccount(string accountNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        AccountMustBeRecognizedByChartOfAccounts.Ensure(
            accounts.ContainsKey(accountNumber),
            accountNumber);

        return accounts[accountNumber];
    }

    private void Apply(ChartOfAccountsAdopted domainEvent)
    {
        Id = domainEvent.ChartOfAccountsId;
        AccountingSubjectId = domainEvent.AccountingSubjectId;
        AdoptedTemplate = domainEvent.Template;
        AdoptedAt = domainEvent.AdoptedAt;

        accounts.Clear();
        foreach (var account in domainEvent.Accounts)
        {
            accounts.Add(account.Number, account);
        }
    }

    private void Apply(AccountAdded domainEvent)
    {
        accounts.Add(
            domainEvent.AccountNumber,
            new Account(domainEvent.AccountNumber, domainEvent.AccountName));
    }

    private void Apply(AccountDeactivated domainEvent)
    {
        accounts[domainEvent.AccountNumber] = accounts[domainEvent.AccountNumber].Deactivate();
    }

    private void Apply(AccountReactivated domainEvent)
    {
        accounts[domainEvent.AccountNumber] = accounts[domainEvent.AccountNumber].Reactivate();
    }

    private static void EnsureTemplate(AdoptedChartOfAccountsTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentException.ThrowIfNullOrWhiteSpace(template.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(template.Name);
    }
}
