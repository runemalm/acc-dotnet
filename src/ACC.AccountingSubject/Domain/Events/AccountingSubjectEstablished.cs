using ACC.AccountingSubject.Domain.Aggregates;

namespace ACC.AccountingSubject.Domain.Events;

public sealed record AccountingSubjectEstablished(
    Guid AccountingSubjectId,
    Guid EstablishedByUserId,
    string Name,
    string OrganizationNumber,
    AccountingSubjectType Type,
    Country Country,
    AccountingMethod AccountingMethod,
    VatReportingPeriod VatReportingPeriod,
    DateTimeOffset EstablishedAt);
