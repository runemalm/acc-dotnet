using ACC.Authority.Application.UseCases.EstablishInitialOwner;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Powers;
using ACC.Authority.Tests.TestKit;
using ACC.BuildingBlocks.Failures;
using Xunit;

namespace ACC.Authority.Tests.UseCases;

public sealed class EstablishInitialOwnerTests
{
    [Fact]
    public void GivenNewAccountingSubject_WhenEstablishingInitialOwner_ThenOwnerRoleAssigned()
    {
        var context = new AuthorityUseCaseTestContext();
        var userId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        var assignedAt = DateTimeOffset.UtcNow;
        context.RecognizeUser(userId);
        context.RecognizeAccountingSubject(accountingSubjectId);

        var result = context.EstablishInitialOwner.Handle(
            new EstablishInitialOwnerCommand(userId, accountingSubjectId),
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
    public void GivenInitialOwnerAlreadyExists_WhenEstablishingInitialOwner_ThenActiveRoleAssignmentMustBeUniqueViolation()
    {
        var context = new AuthorityUseCaseTestContext();
        var userId = Guid.NewGuid();
        var accountingSubjectId = Guid.NewGuid();
        context.RecognizeUser(userId);
        context.RecognizeAccountingSubject(accountingSubjectId);

        context.EstablishInitialOwner.Handle(
            new EstablishInitialOwnerCommand(userId, accountingSubjectId),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<StateConflictException>(() =>
            context.EstablishInitialOwner.Handle(
                new EstablishInitialOwnerCommand(userId, accountingSubjectId),
                DateTimeOffset.UtcNow));

        Assert.Contains("already has active role Owner", exception.Message);
    }

    [Theory]
    [InlineData(Power.PostJournalEntry)]
    [InlineData(Power.ViewJournalEntry)]
    [InlineData(Power.OpenFiscalPeriod)]
    [InlineData(Power.CloseFiscalPeriod)]
    [InlineData(Power.ViewChartOfAccounts)]
    public void GivenInitialOwner_WhenUsingLedger_ThenLedgerPowerGranted(Power power)
    {
        var context = new AuthorityUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var ownerUserId = context.EstablishOwner(accountingSubjectId);

        Assert.True(context.HasPower(ownerUserId, accountingSubjectId, power));
    }
}
