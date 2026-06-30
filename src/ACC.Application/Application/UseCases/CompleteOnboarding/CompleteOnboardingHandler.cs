using ACC.AccountingSubject.Application.UseCases.EstablishAccountingSubject;
using ACC.Authority.Application.UseCases.EstablishInitialOwner;
using ACC.BuildingBlocks.Failures;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;

namespace ACC.Application.Application.UseCases.CompleteOnboarding;

public sealed class CompleteOnboardingHandler
{
    private readonly EstablishAccountingSubjectHandler establishAccountingSubject;
    private readonly EstablishInitialOwnerHandler establishInitialOwner;
    private readonly AdoptChartOfAccountsHandler adoptChartOfAccounts;

    public CompleteOnboardingHandler(
        EstablishAccountingSubjectHandler establishAccountingSubject,
        EstablishInitialOwnerHandler establishInitialOwner,
        AdoptChartOfAccountsHandler adoptChartOfAccounts)
    {
        this.establishAccountingSubject = establishAccountingSubject;
        this.establishInitialOwner = establishInitialOwner;
        this.adoptChartOfAccounts = adoptChartOfAccounts;
    }

    public CompleteOnboardingResult Handle(
        CompleteOnboardingCommand command,
        DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        var accountingSubject = establishAccountingSubject.Handle(new EstablishAccountingSubjectCommand(
            command.ActorUserId,
            command.AccountingSubjectName,
            command.OrganizationNumber,
            command.AccountingSubjectType,
            command.Country,
            command.AccountingMethod,
            command.VatReportingPeriod),
            occurredAt);

        establishInitialOwner.Handle(new EstablishInitialOwnerCommand(
                command.ActorUserId,
                accountingSubject.AccountingSubjectId),
            occurredAt);

        adoptChartOfAccounts.Handle(new AdoptChartOfAccountsCommand(
                command.ActorUserId,
                accountingSubject.AccountingSubjectId,
                command.ChartOfAccountsTemplateId),
            occurredAt);

        return new CompleteOnboardingResult(accountingSubject.AccountingSubjectId);
    }

    private static void ValidateCommand(CompleteOnboardingCommand command)
    {
        if (command.ActorUserId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Completing onboarding must identify the acting user.");
        }

        if (string.IsNullOrWhiteSpace(command.AccountingSubjectName) ||
            string.IsNullOrWhiteSpace(command.OrganizationNumber) ||
            string.IsNullOrWhiteSpace(command.ChartOfAccountsTemplateId))
        {
            throw new ApplicationValidationException(
                "Completing onboarding must describe the accounting subject and selected chart of accounts template.");
        }

        if (!Enum.IsDefined(command.AccountingSubjectType) ||
            !Enum.IsDefined(command.Country) ||
            !Enum.IsDefined(command.AccountingMethod) ||
            !Enum.IsDefined(command.VatReportingPeriod))
        {
            throw new ApplicationValidationException(
                "Completing onboarding must use recognized accounting subject classifications.");
        }
    }
}
