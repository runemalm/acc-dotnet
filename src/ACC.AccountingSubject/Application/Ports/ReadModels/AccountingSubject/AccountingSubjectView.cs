using ACC.AccountingSubject.Domain.Aggregates;

namespace ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;

public sealed record AccountingSubjectView(
    Guid AccountingSubjectId,
    string Name,
    string OrganizationNumber,
    AccountingSubjectType Type,
    Country Country,
    AccountingMethod AccountingMethod,
    VatReportingPeriod VatReportingPeriod);
