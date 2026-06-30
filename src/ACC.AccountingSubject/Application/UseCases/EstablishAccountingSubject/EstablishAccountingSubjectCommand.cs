using ACC.AccountingSubject.Domain.Aggregates;

namespace ACC.AccountingSubject.Application.UseCases.EstablishAccountingSubject;

public sealed record EstablishAccountingSubjectCommand(
    Guid ActorUserId,
    string Name,
    string OrganizationNumber,
    AccountingSubjectType Type,
    Country Country,
    AccountingMethod AccountingMethod,
    VatReportingPeriod VatReportingPeriod);
