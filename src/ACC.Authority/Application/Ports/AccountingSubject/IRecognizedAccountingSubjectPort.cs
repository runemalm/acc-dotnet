namespace ACC.Authority.Application.Ports.AccountingSubject;

public interface IRecognizedAccountingSubjectPort
{
    bool IsRecognizedAccountingSubject(Guid accountingSubjectId);
}
