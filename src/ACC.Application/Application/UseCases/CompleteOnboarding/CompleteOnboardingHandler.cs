using ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;
using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Domain.Aggregates;

namespace ACC.Application.Application.UseCases.CompleteOnboarding;

public sealed class CompleteOnboardingHandler
{
    private readonly CreateAccountingSubjectHandler createAccountingSubject;
    private readonly AssignRoleHandler assignRole;

    public CompleteOnboardingHandler(
        CreateAccountingSubjectHandler createAccountingSubject,
        AssignRoleHandler assignRole)
    {
        this.createAccountingSubject = createAccountingSubject;
        this.assignRole = assignRole;
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

        assignRole.Handle(new AssignRoleCommand(
            command.UserId,
            accountingSubject.AccountingSubjectId,
            Role.Owner));

        return new CompleteOnboardingResult(accountingSubject.AccountingSubjectId);
    }
}
