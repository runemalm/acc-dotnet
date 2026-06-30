using ACC.AccountingSubject.Domain.Events;
using ACC.BuildingBlocks.EventSourcing;

namespace ACC.AccountingSubject.Domain.Aggregates;

public sealed class AccountingSubject : EventSourcedAggregate
{
    private AccountingSubject()
    {
    }

    public Guid Id { get; private set; }

    public Guid EstablishedByUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string OrganizationNumber { get; private set; } = string.Empty;

    public AccountingSubjectType Type { get; private set; }

    public Country Country { get; private set; }

    public AccountingMethod AccountingMethod { get; private set; }

    public VatReportingPeriod VatReportingPeriod { get; private set; }

    public DateTimeOffset EstablishedAt { get; private set; }

    public static AccountingSubject Establish(
        Guid id,
        Guid establishedByUserId,
        string name,
        string organizationNumber,
        AccountingSubjectType type,
        Country country,
        AccountingMethod accountingMethod,
        VatReportingPeriod vatReportingPeriod,
        DateTimeOffset establishedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An accounting subject must have an identity.", nameof(id));
        }

        if (establishedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Establishing an accounting subject must identify the acting user.",
                nameof(establishedByUserId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("An accounting subject must have a name.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(organizationNumber))
        {
            throw new ArgumentException(
                "An accounting subject must have an organization number.",
                nameof(organizationNumber));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        if (!Enum.IsDefined(country))
        {
            throw new ArgumentOutOfRangeException(nameof(country));
        }

        if (!Enum.IsDefined(accountingMethod))
        {
            throw new ArgumentOutOfRangeException(nameof(accountingMethod));
        }

        if (!Enum.IsDefined(vatReportingPeriod))
        {
            throw new ArgumentOutOfRangeException(nameof(vatReportingPeriod));
        }

        var accountingSubject = new AccountingSubject();
        accountingSubject.Raise(new AccountingSubjectEstablished(
            id,
            establishedByUserId,
            name,
            organizationNumber,
            type,
            country,
            accountingMethod,
            vatReportingPeriod,
            establishedAt));

        return accountingSubject;
    }

    public static AccountingSubject Rehydrate(IEnumerable<object> events)
    {
        var accountingSubject = new AccountingSubject();
        accountingSubject.LoadFromHistory(events);

        return accountingSubject;
    }

    protected override void Apply(object domainEvent)
    {
        if (domainEvent is AccountingSubjectEstablished established)
        {
            Apply(established);
        }
    }

    private void Apply(AccountingSubjectEstablished domainEvent)
    {
        Id = domainEvent.AccountingSubjectId;
        EstablishedByUserId = domainEvent.EstablishedByUserId;
        Name = domainEvent.Name;
        OrganizationNumber = domainEvent.OrganizationNumber;
        Type = domainEvent.Type;
        Country = domainEvent.Country;
        AccountingMethod = domainEvent.AccountingMethod;
        VatReportingPeriod = domainEvent.VatReportingPeriod;
        EstablishedAt = domainEvent.EstablishedAt;
    }
}
