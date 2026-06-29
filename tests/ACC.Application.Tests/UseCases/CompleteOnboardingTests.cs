using ACC.AccountingSubject.Domain.Aggregates;
using ACC.Application.Application.UseCases.CompleteOnboarding;
using ACC.Application.Tests.TestKit;
using ACC.Authority.Domain.Aggregates;
using Xunit;

namespace ACC.Application.Tests.UseCases;

public sealed class CompleteOnboardingTests
{
    [Fact]
    public void GivenExistingUserAndSelectedTemplate_WhenCompletingOnboarding_ThenSubjectOwnerAndChartEstablished()
    {
        var context = new ApplicationUseCaseTestContext();
        var userId = Guid.NewGuid();
        const string templateId = "se:bas:k1:2018";
        context.RecognizeUser(userId);
        context.AddTemplate(templateId, "BAS 2018 för K1");

        var result = context.CompleteOnboarding.Handle(
            new CompleteOnboardingCommand(
                userId,
                "Example Business",
                "198001011234",
                AccountingSubjectType.EnskildFirma,
                Country.Sweden,
                AccountingMethod.Cash,
                VatReportingPeriod.Quarterly,
                templateId),
            DateTimeOffset.UtcNow);

        var accountingSubject = context.FindAccountingSubject(result.AccountingSubjectId);
        var owner = Assert.Single(context.FindActiveRoles(userId));
        var chart = context.FindChart(result.AccountingSubjectId);

        Assert.NotNull(accountingSubject);
        Assert.Equal("Example Business", accountingSubject.Name);
        Assert.Equal(result.AccountingSubjectId, owner.AccountingSubjectId);
        Assert.Equal(Role.Owner, owner.Role);
        Assert.NotNull(chart);
        Assert.Equal(templateId, chart.Template.Id);
        Assert.Equal(2, context.FindAccounts(chart.ChartOfAccountsId).Count);
    }
}
