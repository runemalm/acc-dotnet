using ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;
using ACC.AccountingSubject.Domain.Aggregates;
using Xunit;

namespace ACC.AccountingSubject.Tests.Application.UseCases.CreateAccountingSubject;

public sealed class CreateAccountingSubjectHandlerTests
{
    [Fact]
    public void Handle_returns_created_accounting_subject_id()
    {
        var handler = new CreateAccountingSubjectHandler();

        var result = handler.Handle(new CreateAccountingSubjectCommand(
            "Example Business",
            "550101-1234",
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly));

        Assert.NotEqual(Guid.Empty, result.AccountingSubjectId);
    }
}
