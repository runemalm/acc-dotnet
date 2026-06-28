namespace ACC.ChartOfAccounts.Application.Ports.AccountingSubject;

public interface IRecognizedAccountingSubjectPort
{
    bool IsRecognizedAccountingSubject(Guid accountingSubjectId);
}
