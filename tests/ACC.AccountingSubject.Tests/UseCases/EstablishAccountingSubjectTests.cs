using ACC.AccountingSubject.Application.UseCases.EstablishAccountingSubject;
using ACC.AccountingSubject.Domain.Aggregates;
using ACC.AccountingSubject.Domain.Invariants;
using ACC.AccountingSubject.Tests.TestKit;
using ACC.BuildingBlocks.Failures;
using Xunit;

namespace ACC.AccountingSubject.Tests.UseCases;

public sealed class EstablishAccountingSubjectTests
{
    [Fact]
    public void GivenRecognizedUserAndSubjectDetails_WhenEstablishing_ThenAccountingSubjectIsEstablished()
    {
        var context = new AccountingSubjectUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        var establishedAt = DateTimeOffset.UtcNow;
        context.RecognizeUser(actorUserId);

        var result = context.EstablishAccountingSubject.Handle(new EstablishAccountingSubjectCommand(
            actorUserId,
            "Example Business",
            "550101-1234",
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly),
            establishedAt);

        var accountingSubject = context.FindAccountingSubject(result.AccountingSubjectId);
        var aggregate = context.LoadAccountingSubject(result.AccountingSubjectId);

        Assert.NotNull(accountingSubject);
        Assert.Equal(actorUserId, accountingSubject.EstablishedByUserId);
        Assert.Equal("Example Business", accountingSubject.Name);
        Assert.Equal("550101-1234", accountingSubject.OrganizationNumber);
        Assert.Equal(AccountingSubjectType.EnskildFirma, accountingSubject.Type);
        Assert.Equal(Country.Sweden, accountingSubject.Country);
        Assert.Equal(AccountingMethod.Cash, accountingSubject.AccountingMethod);
        Assert.Equal(VatReportingPeriod.Quarterly, accountingSubject.VatReportingPeriod);
        Assert.Equal(establishedAt, accountingSubject.EstablishedAt);
        Assert.Equal(result.AccountingSubjectId, aggregate.Id);
        Assert.Equal(actorUserId, aggregate.EstablishedByUserId);
        Assert.Equal(establishedAt, aggregate.EstablishedAt);
    }

    [Fact]
    public void GivenUnrecognizedUser_WhenEstablishing_ThenRequiredUserNotFound()
    {
        var context = new AccountingSubjectUseCaseTestContext();

        Assert.Throws<RequiredObjectNotFoundException>(() =>
            context.EstablishAccountingSubject.Handle(new EstablishAccountingSubjectCommand(
                Guid.NewGuid(),
                "Example Business",
                "550101-1234",
                AccountingSubjectType.EnskildFirma,
                Country.Sweden,
                AccountingMethod.Cash,
                VatReportingPeriod.Quarterly),
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void GivenExistingOrganizationNumber_WhenEstablishing_ThenOrganizationNumberMustBeUniqueViolation()
    {
        var context = new AccountingSubjectUseCaseTestContext();
        var actorUserId = Guid.NewGuid();
        context.RecognizeUser(actorUserId);
        var command = new EstablishAccountingSubjectCommand(
            actorUserId,
            "Example Business",
            "550101-1234",
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly);

        context.EstablishAccountingSubject.Handle(command, DateTimeOffset.UtcNow);

        Assert.Throws<AccountingSubjectOrganizationNumberMustBeUniqueViolation>(() =>
            context.EstablishAccountingSubject.Handle(command, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void GivenMissingName_WhenEstablishing_ThenApplicationValidationFails()
    {
        var context = new AccountingSubjectUseCaseTestContext();

        Assert.Throws<ApplicationValidationException>(() =>
            context.EstablishAccountingSubject.Handle(new EstablishAccountingSubjectCommand(
                Guid.NewGuid(),
                "",
                "550101-1234",
                AccountingSubjectType.EnskildFirma,
                Country.Sweden,
                AccountingMethod.Cash,
                VatReportingPeriod.Quarterly),
                DateTimeOffset.UtcNow));
    }
}
