using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;
using ACC.Authority.Application.Ports.AccountingSubject;

namespace ACC.Authority.Infrastructure.Adapters.AccountingSubject;

public sealed class RecognizedAccountingSubjectAdapter : IRecognizedAccountingSubjectPort
{
    private readonly IAccountingSubjectStore accountingSubjects;

    public RecognizedAccountingSubjectAdapter(IAccountingSubjectStore accountingSubjects)
    {
        this.accountingSubjects = accountingSubjects;
    }

    public bool IsRecognizedAccountingSubject(Guid accountingSubjectId) =>
        accountingSubjects.Find(accountingSubjectId) is not null;
}
