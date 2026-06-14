using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;
using ACC.AccountingSubject.Domain.Aggregates;
using ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;

namespace ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;

public sealed class CreateAccountingSubjectHandler
{
    private readonly IAccountingSubjectStore accountingSubjects;

    public CreateAccountingSubjectHandler()
        : this(new InMemoryAccountingSubjectStore())
    {
    }

    public CreateAccountingSubjectHandler(IAccountingSubjectStore accountingSubjects)
    {
        this.accountingSubjects = accountingSubjects;
    }

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

        accountingSubjects.Save(new AccountingSubjectView(
            accountingSubject.Id,
            accountingSubject.Name,
            accountingSubject.OrganizationNumber,
            accountingSubject.Type,
            accountingSubject.Country,
            accountingSubject.AccountingMethod,
            accountingSubject.VatReportingPeriod));

        return new CreateAccountingSubjectResult(accountingSubject.Id);
    }
}
