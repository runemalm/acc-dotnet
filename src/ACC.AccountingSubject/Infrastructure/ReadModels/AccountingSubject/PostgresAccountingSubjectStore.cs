using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;

namespace ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;

public sealed class PostgresAccountingSubjectStore : IAccountingSubjectStore
{
    public AccountingSubjectView? Find(Guid accountingSubjectId) =>
        throw new NotSupportedException("Postgres accounting subject read model persistence has not been implemented yet.");

    public AccountingSubjectView? FindByOrganizationNumber(string organizationNumber) =>
        throw new NotSupportedException("Postgres accounting subject read model persistence has not been implemented yet.");

    public void Save(AccountingSubjectView accountingSubject) =>
        throw new NotSupportedException("Postgres accounting subject read model persistence has not been implemented yet.");
}
