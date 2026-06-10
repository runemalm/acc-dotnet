using ACC.AccountingSubject.Domain.Aggregates;
using Xunit;
using Subject = ACC.AccountingSubject.Domain.Aggregates.AccountingSubject;

namespace ACC.AccountingSubject.Tests.Domain.Aggregates;

public sealed class AccountingSubjectTests
{
    [Fact]
    public void Create_returns_accounting_subject()
    {
        var id = Guid.NewGuid();

        var accountingSubject = Subject.Create(
            id,
            "Example Business",
            "550101-1234",
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly);

        Assert.Equal(id, accountingSubject.Id);
        Assert.Equal("Example Business", accountingSubject.Name);
        Assert.Equal("550101-1234", accountingSubject.OrganizationNumber);
        Assert.Equal(AccountingSubjectType.EnskildFirma, accountingSubject.Type);
        Assert.Equal(Country.Sweden, accountingSubject.Country);
        Assert.Equal(AccountingMethod.Cash, accountingSubject.AccountingMethod);
        Assert.Equal(VatReportingPeriod.Quarterly, accountingSubject.VatReportingPeriod);
    }

    [Fact]
    public void Create_requires_identity()
    {
        var exception = Assert.Throws<ArgumentException>(() => Subject.Create(
            Guid.Empty,
            "Example Business",
            "550101-1234",
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly));

        Assert.Equal("id", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_requires_name(string name)
    {
        var exception = Assert.Throws<ArgumentException>(() => Subject.Create(
            Guid.NewGuid(),
            name,
            "550101-1234",
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_requires_organization_number(string organizationNumber)
    {
        var exception = Assert.Throws<ArgumentException>(() => Subject.Create(
            Guid.NewGuid(),
            "Example Business",
            organizationNumber,
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly));

        Assert.Equal("organizationNumber", exception.ParamName);
    }
}
