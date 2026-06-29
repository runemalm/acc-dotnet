using ACC.Authority.Application.Ports.AccountingSubject;
using ACC.Authority.Application.Ports.Identity;
using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Events;
using ACC.Authority.Domain.Invariants;
using ACC.Authority.Infrastructure.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;

namespace ACC.Authority.Application.UseCases.EstablishInitialOwner;

public sealed class EstablishInitialOwnerHandler
{
    private readonly EventSourcedRepository<RoleAssignment> roleAssignments;
    private readonly IRoleAssignmentStore roleAssignmentStore;
    private readonly RoleAssignmentProjection roleAssignmentProjection;
    private readonly IRecognizedUserPort recognizedUsers;
    private readonly IRecognizedAccountingSubjectPort recognizedAccountingSubjects;

    public EstablishInitialOwnerHandler(
        EventSourcedRepository<RoleAssignment> roleAssignments,
        IRoleAssignmentStore roleAssignmentStore,
        RoleAssignmentProjection roleAssignmentProjection,
        IRecognizedUserPort recognizedUsers,
        IRecognizedAccountingSubjectPort recognizedAccountingSubjects)
    {
        this.roleAssignments = roleAssignments;
        this.roleAssignmentStore = roleAssignmentStore;
        this.roleAssignmentProjection = roleAssignmentProjection;
        this.recognizedUsers = recognizedUsers;
        this.recognizedAccountingSubjects = recognizedAccountingSubjects;
    }

    public EstablishInitialOwnerResult Handle(EstablishInitialOwnerCommand command, DateTimeOffset assignedAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        UserMustBeRecognizedForAuthority.Ensure(
            recognizedUsers.IsRecognizedUser(command.ActorUserId),
            command.ActorUserId);
        AccountingSubjectMustBeRecognizedForAuthority.Ensure(
            recognizedAccountingSubjects.IsRecognizedAccountingSubject(command.AccountingSubjectId),
            command.AccountingSubjectId);
        ActiveRoleAssignmentMustBeUnique.Ensure(
            !roleAssignmentStore.ActiveAssignmentExists(
                command.ActorUserId,
                command.AccountingSubjectId,
                Role.Owner),
            command.ActorUserId,
            command.AccountingSubjectId,
            Role.Owner);

        var roleAssignmentId = Guid.NewGuid();
        var roleAssignment = RoleAssignment.EstablishInitialOwner(
            roleAssignmentId,
            command.ActorUserId,
            command.AccountingSubjectId,
            assignedAt);

        var storedEvents = roleAssignments.Save(RoleAssignmentStream(roleAssignmentId), roleAssignment);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<RoleAssigned>()
            .Single();

        roleAssignmentProjection.Project(domainEvent);

        return new EstablishInitialOwnerResult(roleAssignmentId);
    }

    private static void ValidateCommand(EstablishInitialOwnerCommand command)
    {
        if (command.ActorUserId == Guid.Empty || command.AccountingSubjectId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Establishing an initial owner must identify a user and accounting subject.");
        }
    }

    private static StreamId RoleAssignmentStream(Guid roleAssignmentId) =>
        StreamId.For("role-assignment", roleAssignmentId);
}
