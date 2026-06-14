using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Tests.TestKit;
using Xunit;

namespace ACC.Authority.Tests.UseCases;

public sealed class AssignRoleTests
{
    [Fact]
    public void GivenNoExistingRole_WhenAssigningRole_ThenRoleAssigned()
    {
        var context = new AuthorityUseCaseTestContext();
        var userId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var assignedAt = DateTimeOffset.UtcNow;
        var actorUserId = context.EstablishOwner(accountingSubjectId);
        context.RecognizeUser(userId);

        var result = context.AssignRole.Handle(
            new AssignRoleCommand(
                actorUserId,
                userId,
                accountingSubjectId,
                Role.Owner),
            assignedAt);

        var roleAssignment = context.FindRoleAssignment(result.RoleAssignmentId);

        Assert.NotEqual(Guid.Empty, result.RoleAssignmentId);
        Assert.NotNull(roleAssignment);
        Assert.Equal(userId, roleAssignment.UserId);
        Assert.Equal(accountingSubjectId, roleAssignment.AccountingSubjectId);
        Assert.Equal(Role.Owner, roleAssignment.Role);
        Assert.Equal(assignedAt, roleAssignment.AssignedAt);
        Assert.True(roleAssignment.IsActive);
    }

    [Fact]
    public void GivenSameActiveRoleAlreadyAssigned_WhenAssigningRole_ThenActiveRoleAssignmentMustBeUniqueViolation()
    {
        var context = new AuthorityUseCaseTestContext();
        var userId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = context.EstablishOwner(accountingSubjectId);
        context.RecognizeUser(userId);

        context.AssignRole.Handle(
            new AssignRoleCommand(
                actorUserId,
                userId,
                accountingSubjectId,
                Role.Owner),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.AssignRole.Handle(
                new AssignRoleCommand(
                    actorUserId,
                    userId,
                    accountingSubjectId,
                    Role.Owner),
                DateTimeOffset.UtcNow));

        Assert.Contains("already has active role Owner", exception.Message);
    }

    [Fact]
    public void GivenUnrecognizedUser_WhenAssigningRole_ThenUserMustBeRecognizedForAuthorityViolation()
    {
        var context = new AuthorityUseCaseTestContext();
        var userId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = context.EstablishOwner(accountingSubjectId);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.AssignRole.Handle(
                new AssignRoleCommand(
                    actorUserId,
                    userId,
                    accountingSubjectId,
                    Role.Owner),
                DateTimeOffset.UtcNow));

        Assert.Contains("must be recognized before authority can be assigned", exception.Message);
    }

    [Fact]
    public void GivenUnrecognizedAccountingSubject_WhenAssigningRole_ThenAccountingSubjectMustBeRecognizedForAuthorityViolation()
    {
        var context = new AuthorityUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        context.RecognizeUser(actorUserId);
        context.RecognizeUser(userId);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.AssignRole.Handle(
                new AssignRoleCommand(
                    actorUserId,
                    userId,
                    accountingSubjectId,
                    Role.Owner),
                DateTimeOffset.UtcNow));

        Assert.Contains("must be recognized before authority can be assigned", exception.Message);
    }

    [Fact]
    public void GivenActorWithoutAssignRolePower_WhenAssigningRole_ThenActorMustHavePowerViolation()
    {
        var context = new AuthorityUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        context.RecognizeUser(actorUserId);
        context.RecognizeUser(userId);
        context.RecognizeAccountingSubject(accountingSubjectId);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.AssignRole.Handle(
                new AssignRoleCommand(
                    actorUserId,
                    userId,
                    accountingSubjectId,
                    Role.Owner),
                DateTimeOffset.UtcNow));

        Assert.Contains("must have AssignRole power", exception.Message);
    }
}
