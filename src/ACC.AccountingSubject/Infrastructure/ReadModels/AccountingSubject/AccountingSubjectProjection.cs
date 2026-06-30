using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;
using ACC.AccountingSubject.Domain.Events;

namespace ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;

public sealed class AccountingSubjectProjection
{
    private readonly IAccountingSubjectStore accountingSubjects;

    public AccountingSubjectProjection(IAccountingSubjectStore accountingSubjects)
    {
        this.accountingSubjects = accountingSubjects;
    }

    public void Project(AccountingSubjectEstablished domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        accountingSubjects.Save(new AccountingSubjectView(
            domainEvent.AccountingSubjectId,
            domainEvent.EstablishedByUserId,
            domainEvent.Name,
            domainEvent.OrganizationNumber,
            domainEvent.Type,
            domainEvent.Country,
            domainEvent.AccountingMethod,
            domainEvent.VatReportingPeriod,
            domainEvent.EstablishedAt));
    }
}
