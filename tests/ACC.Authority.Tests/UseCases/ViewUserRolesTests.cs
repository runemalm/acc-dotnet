using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.RevokeRole;
using ACC.Authority.Application.UseCases.ViewUserRoles;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Tests.TestKit;
using Xunit;

namespace ACC.Authority.Tests.UseCases;

public sealed class ViewUserRolesTests
{
    [Fact]
    public void GivenOwnRoles_WhenViewingUserRoles_ThenActiveRolesReturned()
    {
        var context = new AuthorityUseCaseTestContext();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var activeAccountingSubjectId = Guid.NewGuid();
        var revokedAccountingSubjectId = Guid.NewGuid();
        var activeSubjectOwnerId = context.EstablishOwner(activeAccountingSubjectId);
        var revokedSubjectOwnerId = context.EstablishOwner(revokedAccountingSubjectId);
        context.RecognizeUser(userId);
        context.RecognizeUser(otherUserId);

        var activeRole = context.AssignRole.Handle(
            new AssignRoleCommand(
                activeSubjectOwnerId,
                userId,
                activeAccountingSubjectId,
                Role.Owner),
            DateTimeOffset.UtcNow);

        context.AssignRole.Handle(
            new AssignRoleCommand(
                activeSubjectOwnerId,
                otherUserId,
                activeAccountingSubjectId,
                Role.Owner),
            DateTimeOffset.UtcNow);

        var revokedRole = context.AssignRole.Handle(
            new AssignRoleCommand(
                revokedSubjectOwnerId,
                userId,
                revokedAccountingSubjectId,
                Role.Owner),
            DateTimeOffset.UtcNow);

        context.RevokeRole.Handle(
            new RevokeRoleCommand(revokedSubjectOwnerId, revokedRole.RoleAssignmentId),
            DateTimeOffset.UtcNow);

        var response = context.ViewUserRoles.Handle(new ViewUserRolesQuery(userId));

        var role = Assert.Single(response.Roles);
        Assert.Equal(userId, response.UserId);
        Assert.Equal(activeRole.RoleAssignmentId, role.RoleAssignmentId);
        Assert.Equal(activeAccountingSubjectId, role.AccountingSubjectId);
        Assert.Equal(nameof(Role.Owner), role.Role);
    }
}
