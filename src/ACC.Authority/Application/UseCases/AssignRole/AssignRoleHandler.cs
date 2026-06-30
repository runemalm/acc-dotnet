using ACC.Authority.Application.Policies;
using ACC.Authority.Application.Ports.AccountingSubject;
using ACC.Authority.Application.Ports.Identity;
using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Events;
using ACC.Authority.Domain.Invariants;
using ACC.Authority.Domain.Powers;
using ACC.Authority.Infrastructure.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;

namespace ACC.Authority.Application.UseCases.AssignRole;

public sealed class AssignRoleHandler
{
    private readonly EventSourcedRepository<RoleAssignment> roleAssignments;
    private readonly IRoleAssignmentStore roleAssignmentStore;
    private readonly RoleAssignmentProjection roleAssignmentProjection;
    private readonly IRecognizedUserPort recognizedUsers;
    private readonly IRecognizedAccountingSubjectPort recognizedAccountingSubjects;
    private readonly IAuthorityPolicy authorityPolicy;

    public AssignRoleHandler(
        EventSourcedRepository<RoleAssignment> roleAssignments,
        IRoleAssignmentStore roleAssignmentStore,
        RoleAssignmentProjection roleAssignmentProjection,
        IRecognizedUserPort recognizedUsers,
        IRecognizedAccountingSubjectPort recognizedAccountingSubjects,
        IAuthorityPolicy authorityPolicy)
    {
        this.roleAssignments = roleAssignments;
        this.roleAssignmentStore = roleAssignmentStore;
        this.roleAssignmentProjection = roleAssignmentProjection;
        this.recognizedUsers = recognizedUsers;
        this.recognizedAccountingSubjects = recognizedAccountingSubjects;
        this.authorityPolicy = authorityPolicy;
    }

    public AssignRoleResult Handle(AssignRoleCommand command, DateTimeOffset assignedAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        if (!recognizedUsers.IsRecognizedUser(command.ActorUserId))
        {
            throw new RequiredObjectNotFoundException(
                $"Acting user {command.ActorUserId} is required to assign a role.");
        }

        if (!recognizedUsers.IsRecognizedUser(command.UserId))
        {
            throw new RequiredObjectNotFoundException(
                $"Receiving user {command.UserId} is required to assign a role.");
        }

        if (!recognizedAccountingSubjects.IsRecognizedAccountingSubject(command.AccountingSubjectId))
        {
            throw new RequiredObjectNotFoundException(
                $"Accounting subject {command.AccountingSubjectId} is required to assign a role.");
        }

        if (!authorityPolicy.HasPower(
                command.ActorUserId,
                command.AccountingSubjectId,
                Power.AssignRole))
        {
            throw new AuthorizationDeniedException(
                $"User {command.ActorUserId} must have AssignRole power for accounting subject {command.AccountingSubjectId}.");
        }

        ActiveRoleAssignmentMustBeUnique.Ensure(
            !roleAssignmentStore.ActiveAssignmentExists(
                command.UserId,
                command.AccountingSubjectId,
                command.Role),
            command.UserId,
            command.AccountingSubjectId,
            command.Role);

        var roleAssignmentId = Guid.NewGuid();
        var roleAssignment = RoleAssignment.Assign(
            roleAssignmentId,
            command.ActorUserId,
            command.UserId,
            command.AccountingSubjectId,
            command.Role,
            assignedAt);

        var storedEvents = roleAssignments.Save(RoleAssignmentStream(roleAssignmentId), roleAssignment);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<RoleAssigned>()
            .Single();

        roleAssignmentProjection.Project(domainEvent);

        return new AssignRoleResult(roleAssignmentId);
    }

    private static StreamId RoleAssignmentStream(Guid roleAssignmentId) =>
        StreamId.For("role-assignment", roleAssignmentId);

    private static void ValidateCommand(AssignRoleCommand command)
    {
        if (command.UserId == Guid.Empty ||
            command.ActorUserId == Guid.Empty ||
            command.AccountingSubjectId == Guid.Empty ||
            !Enum.IsDefined(command.Role))
        {
            throw new ApplicationValidationException(
                "A role assignment must identify a user, accounting subject, and recognized role.");
        }
    }
}
