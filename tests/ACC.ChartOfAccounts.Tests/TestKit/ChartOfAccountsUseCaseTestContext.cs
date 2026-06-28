using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Application.Ports.Templates;
using ACC.ChartOfAccounts.Application.UseCases.AddAccount;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ReactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccountsTemplates;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Tests.TestKit;

internal sealed class ChartOfAccountsUseCaseTestContext
{
    private readonly InMemoryChartOfAccountsStore chartStore = new();
    private readonly InMemoryAccountStore accountStore = new();
    private readonly TestChartOfAccountsTemplateCatalog templates = new();
    private readonly TestRecognizedAccountingSubjectPort recognizedAccountingSubjects = new();
    private readonly TestChartOfAccountsAuthorityPort authority = new();

    public ChartOfAccountsUseCaseTestContext()
    {
        var eventStore = new InMemoryEventStore();
        var charts = new EventSourcedRepository<Domain.Aggregates.ChartOfAccounts>(
            eventStore,
            Domain.Aggregates.ChartOfAccounts.Rehydrate);
        var projection = new ChartOfAccountsProjection(chartStore, accountStore);

        AdoptChartOfAccounts = new AdoptChartOfAccountsHandler(
            charts,
            chartStore,
            templates,
            recognizedAccountingSubjects,
            authority,
            projection);
        AddAccount = new AddAccountHandler(charts, authority, projection);
        DeactivateAccount = new DeactivateAccountHandler(charts, authority, projection);
        ReactivateAccount = new ReactivateAccountHandler(charts, authority, projection);
        ViewChartOfAccounts = new ViewChartOfAccountsHandler(chartStore, accountStore);
        ViewChartOfAccountsTemplates = new ViewChartOfAccountsTemplatesHandler(templates);
    }

    public AdoptChartOfAccountsHandler AdoptChartOfAccounts { get; }

    public AddAccountHandler AddAccount { get; }

    public DeactivateAccountHandler DeactivateAccount { get; }

    public ReactivateAccountHandler ReactivateAccount { get; }

    public ViewChartOfAccountsHandler ViewChartOfAccounts { get; }

    public ViewChartOfAccountsTemplatesHandler ViewChartOfAccountsTemplates { get; }

    public ChartOfAccountsTemplate AddTemplate(
        string id = "test-template",
        string name = "Test chart of accounts")
    {
        var template = new ChartOfAccountsTemplate(
            id,
            name,
            [
                new TemplateAccount("1000", "Assets"),
                new TemplateAccount("2000", "Equity and liabilities")
            ]);
        templates.Add(template);
        return template;
    }

    public void RecognizeAccountingSubject(Guid accountingSubjectId) =>
        recognizedAccountingSubjects.Recognize(accountingSubjectId);

    public void AllowAdoption(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowAdoption(actorUserId, accountingSubjectId);

    public void AllowManagement(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowManagement(actorUserId, accountingSubjectId);

    public Guid AdoptChart(Guid accountingSubjectId, Guid actorUserId)
    {
        const string templateId = "test-template";
        AddTemplate(templateId);
        RecognizeAccountingSubject(accountingSubjectId);
        AllowAdoption(actorUserId, accountingSubjectId);
        AllowManagement(actorUserId, accountingSubjectId);

        return AdoptChartOfAccounts.Handle(
            new AdoptChartOfAccountsCommand(actorUserId, accountingSubjectId, templateId),
            DateTimeOffset.UtcNow).ChartOfAccountsId;
    }

    public ChartOfAccountsView? FindChartFor(Guid accountingSubjectId) =>
        chartStore.FindFor(accountingSubjectId);

    public AccountView? FindAccount(Guid accountingSubjectId, string accountNumber) =>
        accountStore.Find(accountingSubjectId, accountNumber);
}
