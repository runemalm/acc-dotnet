namespace ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;

public interface IAccountingSubjectStore
{
    AccountingSubjectView? Find(Guid accountingSubjectId);

    void Save(AccountingSubjectView accountingSubject);
}
