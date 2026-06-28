using ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;
using ACC.Authority.Application.UseCases.EstablishInitialOwner;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;

namespace ACC.Application.Application.UseCases.CompleteOnboarding;

public sealed class CompleteOnboardingHandler
{
    private readonly CreateAccountingSubjectHandler createAccountingSubject;
    private readonly EstablishInitialOwnerHandler establishInitialOwner;
    private readonly AdoptChartOfAccountsHandler adoptChartOfAccounts;

    public CompleteOnboardingHandler(
        CreateAccountingSubjectHandler createAccountingSubject,
        EstablishInitialOwnerHandler establishInitialOwner,
        AdoptChartOfAccountsHandler adoptChartOfAccounts)
    {
        this.createAccountingSubject = createAccountingSubject;
        this.establishInitialOwner = establishInitialOwner;
        this.adoptChartOfAccounts = adoptChartOfAccounts;
    }

    public CompleteOnboardingResult Handle(CompleteOnboardingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var occurredAt = DateTimeOffset.UtcNow;

        var accountingSubject = createAccountingSubject.Handle(new CreateAccountingSubjectCommand(
            command.AccountingSubjectName,
            command.OrganizationNumber,
            command.AccountingSubjectType,
            command.Country,
            command.AccountingMethod,
            command.VatReportingPeriod));

        establishInitialOwner.Handle(new EstablishInitialOwnerCommand(
                command.UserId,
                accountingSubject.AccountingSubjectId),
            occurredAt);

        adoptChartOfAccounts.Handle(new AdoptChartOfAccountsCommand(
                command.UserId,
                accountingSubject.AccountingSubjectId,
                command.ChartOfAccountsTemplateId),
            occurredAt);

        return new CompleteOnboardingResult(accountingSubject.AccountingSubjectId);
    }
}
