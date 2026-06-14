using ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;
using ACC.Authority.Application.UseCases.EstablishInitialOwner;

namespace ACC.Application.Application.UseCases.CompleteOnboarding;

public sealed class CompleteOnboardingHandler
{
    private readonly CreateAccountingSubjectHandler createAccountingSubject;
    private readonly EstablishInitialOwnerHandler establishInitialOwner;

    public CompleteOnboardingHandler(
        CreateAccountingSubjectHandler createAccountingSubject,
        EstablishInitialOwnerHandler establishInitialOwner)
    {
        this.createAccountingSubject = createAccountingSubject;
        this.establishInitialOwner = establishInitialOwner;
    }

    public CompleteOnboardingResult Handle(CompleteOnboardingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var accountingSubject = createAccountingSubject.Handle(new CreateAccountingSubjectCommand(
            command.AccountingSubjectName,
            command.OrganizationNumber,
            command.AccountingSubjectType,
            command.Country,
            command.AccountingMethod,
            command.VatReportingPeriod));

        establishInitialOwner.Handle(new EstablishInitialOwnerCommand(
            command.UserId,
            accountingSubject.AccountingSubjectId), DateTimeOffset.UtcNow);

        return new CompleteOnboardingResult(accountingSubject.AccountingSubjectId);
    }
}
