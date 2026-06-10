using ACC.AccountingSubject.Domain.Aggregates;

namespace ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;

public sealed record CreateAccountingSubjectCommand(
    string Name,
    string OrganizationNumber,
    AccountingSubjectType Type,
    Country Country,
    AccountingMethod AccountingMethod,
    VatReportingPeriod VatReportingPeriod);
