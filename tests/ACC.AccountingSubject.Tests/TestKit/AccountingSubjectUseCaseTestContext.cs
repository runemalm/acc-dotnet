using ACC.AccountingSubject.Application.Ports.Identity;
using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;
using ACC.AccountingSubject.Application.UseCases.EstablishAccountingSubject;
using ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using AccountingSubjectAggregate = ACC.AccountingSubject.Domain.Aggregates.AccountingSubject;

namespace ACC.AccountingSubject.Tests.TestKit;

internal sealed class AccountingSubjectUseCaseTestContext
{
    private readonly EventSourcedRepository<AccountingSubjectAggregate> accountingSubjects;
    private readonly InMemoryAccountingSubjectStore accountingSubjectStore = new();
    private readonly TestRecognizedUserPort recognizedUsers = new();

    public AccountingSubjectUseCaseTestContext()
    {
        accountingSubjects = new EventSourcedRepository<AccountingSubjectAggregate>(
            new InMemoryEventStore(),
            AccountingSubjectAggregate.Rehydrate);
        EstablishAccountingSubject = new EstablishAccountingSubjectHandler(
            accountingSubjects,
            accountingSubjectStore,
            new AccountingSubjectProjection(accountingSubjectStore),
            recognizedUsers);
    }

    public EstablishAccountingSubjectHandler EstablishAccountingSubject { get; }

    public AccountingSubjectView? FindAccountingSubject(Guid accountingSubjectId) =>
        accountingSubjectStore.Find(accountingSubjectId);

    public AccountingSubjectAggregate LoadAccountingSubject(Guid accountingSubjectId) =>
        accountingSubjects.Load(StreamId.For("accounting-subject", accountingSubjectId));

    public void RecognizeUser(Guid userId) =>
        recognizedUsers.Recognize(userId);

    private sealed class TestRecognizedUserPort : IRecognizedUserPort
    {
        private readonly HashSet<Guid> userIds = [];

        public bool IsRecognizedUser(Guid userId) =>
            userIds.Contains(userId);

        public void Recognize(Guid userId) =>
            userIds.Add(userId);
    }
}
