using ACC.AccountingSubject.Domain.Aggregates;

namespace ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;

public sealed class CreateAccountingSubjectHandler
{
    public CreateAccountingSubjectResult Handle(CreateAccountingSubjectCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var accountingSubject = Domain.Aggregates.AccountingSubject.Create(
            Guid.NewGuid(),
            command.Name,
            command.OrganizationNumber,
            command.Type,
            command.Country,
            command.AccountingMethod,
            command.VatReportingPeriod);

        // TODO: Persist the created accounting subject.
        return new CreateAccountingSubjectResult(accountingSubject.Id);
    }
}
