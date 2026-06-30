using ACC.AccountingSubject.Application.Ports.Identity;
using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;
using ACC.AccountingSubject.Domain.Events;
using ACC.AccountingSubject.Domain.Invariants;
using ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;

namespace ACC.AccountingSubject.Application.UseCases.EstablishAccountingSubject;

public sealed class EstablishAccountingSubjectHandler
{
    private readonly EventSourcedRepository<Domain.Aggregates.AccountingSubject> accountingSubjects;
    private readonly IAccountingSubjectStore accountingSubjectStore;
    private readonly AccountingSubjectProjection accountingSubjectProjection;
    private readonly IRecognizedUserPort recognizedUsers;

    public EstablishAccountingSubjectHandler(
        EventSourcedRepository<Domain.Aggregates.AccountingSubject> accountingSubjects,
        IAccountingSubjectStore accountingSubjectStore,
        AccountingSubjectProjection accountingSubjectProjection,
        IRecognizedUserPort recognizedUsers)
    {
        this.accountingSubjects = accountingSubjects;
        this.accountingSubjectStore = accountingSubjectStore;
        this.accountingSubjectProjection = accountingSubjectProjection;
        this.recognizedUsers = recognizedUsers;
    }

    public EstablishAccountingSubjectResult Handle(
        EstablishAccountingSubjectCommand command,
        DateTimeOffset establishedAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        if (!recognizedUsers.IsRecognizedUser(command.ActorUserId))
        {
            throw new RequiredObjectNotFoundException(
                $"User {command.ActorUserId} must be recognized before establishing an accounting subject.");
        }

        AccountingSubjectOrganizationNumberMustBeUnique.Ensure(
            accountingSubjectStore.FindByOrganizationNumber(command.OrganizationNumber) is null);

        var accountingSubject = Domain.Aggregates.AccountingSubject.Establish(
            Guid.NewGuid(),
            command.ActorUserId,
            command.Name,
            command.OrganizationNumber,
            command.Type,
            command.Country,
            command.AccountingMethod,
            command.VatReportingPeriod,
            establishedAt);

        var storedEvents = accountingSubjects.Save(
            AccountingSubjectStream(accountingSubject.Id),
            accountingSubject);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<AccountingSubjectEstablished>()
            .Single();

        accountingSubjectProjection.Project(domainEvent);

        return new EstablishAccountingSubjectResult(accountingSubject.Id);
    }

    private static void ValidateCommand(EstablishAccountingSubjectCommand command)
    {
        if (command.ActorUserId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Establishing an accounting subject must identify the acting user.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ApplicationValidationException(
                "An accounting subject must have a name.");
        }

        if (string.IsNullOrWhiteSpace(command.OrganizationNumber))
        {
            throw new ApplicationValidationException(
                "An accounting subject must have an organization number.");
        }

        if (!Enum.IsDefined(command.Type) ||
            !Enum.IsDefined(command.Country) ||
            !Enum.IsDefined(command.AccountingMethod) ||
            !Enum.IsDefined(command.VatReportingPeriod))
        {
            throw new ApplicationValidationException(
                "An accounting subject must use recognized classifications.");
        }
    }

    private static StreamId AccountingSubjectStream(Guid accountingSubjectId) =>
        StreamId.For("accounting-subject", accountingSubjectId);
}
