using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;
using ACC.ChartOfAccounts.Domain.Invariants;
using ACC.ChartOfAccounts.Tests.TestKit;
using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.Failures;
using Xunit;

namespace ACC.ChartOfAccounts.Tests.UseCases;

public sealed class AdoptChartOfAccountsTests
{
    [Fact]
    public void GivenUnrecognizedAccountingSubject_WhenAdoptingChart_ThenRequiredAccountingSubjectNotFound()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var template = context.AddTemplate();

        var exception = Assert.Throws<RequiredObjectNotFoundException>(() =>
            context.AdoptChartOfAccounts.Handle(
                new AdoptChartOfAccountsCommand(
                    actorUserId,
                    accountingSubjectId,
                    template.Id),
                DateTimeOffset.UtcNow));

        Assert.Contains("is required to adopt", exception.Message);
    }

    [Fact]
    public void GivenRecognizedSubjectAndTemplate_WhenAdoptingChart_ThenChartOfAccountsAdopted()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var template = context.AddTemplate();
        context.RecognizeAccountingSubject(accountingSubjectId);
        context.AllowAdoption(actorUserId, accountingSubjectId);

        var result = context.AdoptChartOfAccounts.Handle(
            new AdoptChartOfAccountsCommand(
                actorUserId,
                accountingSubjectId,
                template.Id),
            DateTimeOffset.UtcNow);

        var chart = context.FindChartFor(accountingSubjectId);
        Assert.NotEqual(Guid.Empty, result.ChartOfAccountsId);
        Assert.NotNull(chart);
        Assert.Equal(result.ChartOfAccountsId, chart.ChartOfAccountsId);
        Assert.Equal(template.Id, chart.Template.Id);
        Assert.Equal(template.Name, chart.Template.Name);
        Assert.NotNull(context.FindAccount(accountingSubjectId, "1000"));
        Assert.NotNull(context.FindAccount(accountingSubjectId, "2000"));
    }

    [Fact]
    public void GivenExistingOperativeChart_WhenAdoptingAnother_ThenAtMostOneOperativeChartViolation()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        context.AdoptChart(accountingSubjectId, actorUserId);
        context.AddTemplate("another-template", "Another chart");

        var exception = Assert.Throws<AccountingSubjectMustHaveAtMostOneOperativeChartOfAccountsViolation>(() =>
            context.AdoptChartOfAccounts.Handle(
                new AdoptChartOfAccountsCommand(
                    actorUserId,
                    accountingSubjectId,
                    "another-template"),
                DateTimeOffset.UtcNow));

        Assert.Contains("already has an operative chart of accounts", exception.Message);
    }

    [Fact]
    public void GivenActorWithoutPower_WhenAdoptingChart_ThenAuthorizationDenied()
    {
        var context = new ChartOfAccountsUseCaseTestContext();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var template = context.AddTemplate();
        context.RecognizeAccountingSubject(accountingSubjectId);

        var exception = Assert.Throws<AuthorizationDeniedException>(() =>
            context.AdoptChartOfAccounts.Handle(
                new AdoptChartOfAccountsCommand(
                    actorUserId,
                    accountingSubjectId,
                    template.Id),
                DateTimeOffset.UtcNow));

        Assert.Contains("must have power to adopt a chart of accounts", exception.Message);
    }
}
