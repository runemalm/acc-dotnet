using ACC.Authority.Application.Policies;
using ACC.Authority.Application.Ports.AccountingSubject;
using ACC.Authority.Application.Ports.Identity;
using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Events;
using ACC.Authority.Domain.Invariants;
using ACC.Authority.Domain.Powers;
using ACC.Authority.Infrastructure.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.EventSourcing;

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

        UserMustBeRecognizedForAuthority.Ensure(
            recognizedUsers.IsRecognizedUser(command.ActorUserId),
            command.ActorUserId);
        UserMustBeRecognizedForAuthority.Ensure(
            recognizedUsers.IsRecognizedUser(command.UserId),
            command.UserId);
        AccountingSubjectMustBeRecognizedForAuthority.Ensure(
            recognizedAccountingSubjects.IsRecognizedAccountingSubject(command.AccountingSubjectId),
            command.AccountingSubjectId);
        ActorMustHavePower.Ensure(
            authorityPolicy.HasPower(
                command.ActorUserId,
                command.AccountingSubjectId,
                Power.AssignRole),
            command.ActorUserId,
            command.AccountingSubjectId,
            Power.AssignRole);
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
}
