using ACC.Authority.Application.Policies;
using ACC.Authority.Application.Ports.AccountingSubject;
using ACC.Authority.Application.Ports.Identity;
using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.EstablishInitialOwner;
using ACC.Authority.Application.UseCases.RevokeRole;
using ACC.Authority.Application.UseCases.ViewUserRoles;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Powers;
using ACC.Authority.Infrastructure.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;

namespace ACC.Authority.Tests.TestKit;

internal sealed class AuthorityUseCaseTestContext
{
    private readonly InMemoryRoleAssignmentStore roleAssignmentStore = new();
    private readonly TestRecognizedUserPort recognizedUsers = new();
    private readonly TestRecognizedAccountingSubjectPort recognizedAccountingSubjects = new();
    private readonly AuthorityPolicy authorityPolicy;

    public AuthorityUseCaseTestContext()
    {
        var eventStore = new InMemoryEventStore();
        var roleAssignments = new EventSourcedRepository<RoleAssignment>(
            eventStore,
            RoleAssignment.Rehydrate);
        var roleAssignmentProjection = new RoleAssignmentProjection(roleAssignmentStore);
        authorityPolicy = new AuthorityPolicy(
            roleAssignmentStore,
            new RolePowerPolicy());

        EstablishInitialOwner = new EstablishInitialOwnerHandler(
            roleAssignments,
            roleAssignmentStore,
            roleAssignmentProjection,
            recognizedUsers,
            recognizedAccountingSubjects);

        AssignRole = new AssignRoleHandler(
            roleAssignments,
            roleAssignmentStore,
            roleAssignmentProjection,
            recognizedUsers,
            recognizedAccountingSubjects,
            authorityPolicy);

        RevokeRole = new RevokeRoleHandler(
            roleAssignments,
            roleAssignmentProjection,
            recognizedUsers,
            authorityPolicy);

        ViewUserRoles = new ViewUserRolesHandler(roleAssignmentStore);
    }

    public EstablishInitialOwnerHandler EstablishInitialOwner { get; }

    public AssignRoleHandler AssignRole { get; }

    public RevokeRoleHandler RevokeRole { get; }

    public ViewUserRolesHandler ViewUserRoles { get; }

    public RoleAssignmentView? FindRoleAssignment(Guid roleAssignmentId) =>
        roleAssignmentStore.Find(roleAssignmentId);

    public bool HasPower(Guid actorUserId, Guid accountingSubjectId, Power power) =>
        authorityPolicy.HasPower(actorUserId, accountingSubjectId, power);

    public void RecognizeUser(Guid userId) =>
        recognizedUsers.Recognize(userId);

    public void RecognizeAccountingSubject(Guid accountingSubjectId) =>
        recognizedAccountingSubjects.Recognize(accountingSubjectId);

    public Guid EstablishOwner(Guid accountingSubjectId)
    {
        var ownerUserId = Guid.NewGuid();
        RecognizeUser(ownerUserId);
        RecognizeAccountingSubject(accountingSubjectId);

        EstablishInitialOwner.Handle(
            new EstablishInitialOwnerCommand(ownerUserId, accountingSubjectId),
            DateTimeOffset.UtcNow);

        return ownerUserId;
    }

    public void RecognizeAuthorityParticipants(params Guid[] ids)
    {
        foreach (var id in ids)
        {
            RecognizeUser(id);
            RecognizeAccountingSubject(id);
        }
    }

    private sealed class TestRecognizedUserPort : IRecognizedUserPort
    {
        private readonly HashSet<Guid> userIds = [];

        public bool IsRecognizedUser(Guid userId) =>
            userIds.Contains(userId);

        public void Recognize(Guid userId) =>
            userIds.Add(userId);
    }

    private sealed class TestRecognizedAccountingSubjectPort : IRecognizedAccountingSubjectPort
    {
        private readonly HashSet<Guid> accountingSubjectIds = [];

        public bool IsRecognizedAccountingSubject(Guid accountingSubjectId) =>
            accountingSubjectIds.Contains(accountingSubjectId);

        public void Recognize(Guid accountingSubjectId) =>
            accountingSubjectIds.Add(accountingSubjectId);
    }
}
