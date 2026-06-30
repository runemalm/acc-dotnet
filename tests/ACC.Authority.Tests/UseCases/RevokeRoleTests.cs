using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.RevokeRole;
using ACC.Authority.Domain.Invariants;
using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.Failures;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Tests.TestKit;
using Xunit;

namespace ACC.Authority.Tests.UseCases;

public sealed class RevokeRoleTests
{
    [Fact]
    public void GivenIncompleteRevocation_WhenRevokingRole_ThenApplicationValidationFails()
    {
        var context = new AuthorityUseCaseTestContext();

        var exception = Assert.Throws<ApplicationValidationException>(() =>
            context.RevokeRole.Handle(
                new RevokeRoleCommand(Guid.Empty, Guid.NewGuid()),
                DateTimeOffset.UtcNow));

        Assert.Contains("must identify", exception.Message);
    }

    [Fact]
    public void GivenMissingRoleAssignment_WhenRevokingRole_ThenRequiredObjectNotFound()
    {
        var context = new AuthorityUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        context.RecognizeUser(actorUserId);

        var exception = Assert.Throws<RequiredObjectNotFoundException>(() =>
            context.RevokeRole.Handle(
                new RevokeRoleCommand(actorUserId, Guid.NewGuid()),
                DateTimeOffset.UtcNow));

        Assert.Contains("must exist", exception.Message);
    }

    [Fact]
    public void GivenActiveRoleAssignment_WhenRevokingRole_ThenRoleRevoked()
    {
        var context = new AuthorityUseCaseTestContext();
        var assigned = AssignOwnerRole(context);
        var revokedAt = DateTimeOffset.UtcNow;

        context.RevokeRole.Handle(
            new RevokeRoleCommand(assigned.ActorUserId, assigned.RoleAssignmentId),
            revokedAt);

        var roleAssignment = context.FindRoleAssignment(assigned.RoleAssignmentId);

        Assert.NotNull(roleAssignment);
        Assert.Equal(revokedAt, roleAssignment.RevokedAt);
        Assert.False(roleAssignment.IsActive);
    }

    [Fact]
    public void GivenRevokedRoleAssignment_WhenRevokingRole_ThenRoleAssignmentMustBeActiveToRevokeViolation()
    {
        var context = new AuthorityUseCaseTestContext();
        var assigned = AssignOwnerRole(context);

        context.RevokeRole.Handle(
            new RevokeRoleCommand(assigned.ActorUserId, assigned.RoleAssignmentId),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<RoleAssignmentMustBeActiveToRevokeViolation>(() =>
            context.RevokeRole.Handle(
                new RevokeRoleCommand(assigned.ActorUserId, assigned.RoleAssignmentId),
                DateTimeOffset.UtcNow));

        Assert.Equal("A role assignment must be active before it can be revoked.", exception.Message);
    }

    [Fact]
    public void GivenActorWithoutRevokeRolePower_WhenRevokingRole_ThenAuthorizationDenied()
    {
        var context = new AuthorityUseCaseTestContext();
        var assigned = AssignOwnerRole(context);
        var actorUserId = Guid.NewGuid();
        context.RecognizeUser(actorUserId);

        var exception = Assert.Throws<AuthorizationDeniedException>(() =>
            context.RevokeRole.Handle(
                new RevokeRoleCommand(actorUserId, assigned.RoleAssignmentId),
                DateTimeOffset.UtcNow));

        Assert.Contains("must have RevokeRole power", exception.Message);
    }

    private static AssignedRole AssignOwnerRole(AuthorityUseCaseTestContext context)
    {
        var userId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = context.EstablishOwner(accountingSubjectId);
        context.RecognizeUser(userId);

        var result = context.AssignRole.Handle(
            new AssignRoleCommand(
                actorUserId,
                userId,
                accountingSubjectId,
                Role.Owner),
            DateTimeOffset.UtcNow);

        return new AssignedRole(result.RoleAssignmentId, actorUserId);
    }

    private sealed record AssignedRole(Guid RoleAssignmentId, Guid ActorUserId);
}
