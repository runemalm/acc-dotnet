using ACC.AccountingSubject.Domain.Aggregates;

namespace ACC.Application.Application.UseCases.CompleteOnboarding;

public sealed record CompleteOnboardingCommand(
    Guid ActorUserId,
    string AccountingSubjectName,
    string OrganizationNumber,
    AccountingSubjectType AccountingSubjectType,
    Country Country,
    AccountingMethod AccountingMethod,
    VatReportingPeriod VatReportingPeriod,
    string ChartOfAccountsTemplateId);
