namespace ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;

public interface IAccountingSubjectStore
{
    AccountingSubjectView? Find(Guid accountingSubjectId);

    AccountingSubjectView? FindByOrganizationNumber(string organizationNumber);

    void Save(AccountingSubjectView accountingSubject);
}
