namespace ACC.AccountingSubject.Domain.Aggregates;

public sealed class AccountingSubject
{
    private AccountingSubject(
        Guid id,
        string name,
        string organizationNumber,
        AccountingSubjectType type,
        Country country,
        AccountingMethod accountingMethod,
        VatReportingPeriod vatReportingPeriod)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An accounting subject must have an identity.", nameof(id));
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

        Id = id;
        Name = name;
        OrganizationNumber = organizationNumber;
        Type = type;
        Country = country;
        AccountingMethod = accountingMethod;
        VatReportingPeriod = vatReportingPeriod;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string OrganizationNumber { get; }

    public AccountingSubjectType Type { get; }

    public Country Country { get; }

    public AccountingMethod AccountingMethod { get; }

    public VatReportingPeriod VatReportingPeriod { get; }

    public static AccountingSubject Create(
        Guid id,
        string name,
        string organizationNumber,
        AccountingSubjectType type,
        Country country,
        AccountingMethod accountingMethod,
        VatReportingPeriod vatReportingPeriod) =>
        new(
            id,
            name,
            organizationNumber,
            type,
            country,
            accountingMethod,
            vatReportingPeriod);
}
