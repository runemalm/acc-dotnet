using System.Collections.Concurrent;
using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;

namespace ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;

public sealed class InMemoryAccountingSubjectStore : IAccountingSubjectStore
{
    private readonly ConcurrentDictionary<Guid, AccountingSubjectView> accountingSubjects = new();

    public AccountingSubjectView? Find(Guid accountingSubjectId) =>
        accountingSubjects.GetValueOrDefault(accountingSubjectId);

    public void Save(AccountingSubjectView accountingSubject)
    {
        ArgumentNullException.ThrowIfNull(accountingSubject);

        accountingSubjects[accountingSubject.AccountingSubjectId] = accountingSubject;
    }
}
