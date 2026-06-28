using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;
using ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;
using ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;
using ACC.Application.Application.UseCases.CompleteOnboarding;
using ACC.Authority.Application.Policies;
using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Application.UseCases.EstablishInitialOwner;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Powers;
using ACC.Authority.Infrastructure.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Application.Ports.Templates;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;
using ACC.ChartOfAccounts.Infrastructure.Adapters.Authority;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;
using AuthorityRecognizedAccountingSubjectAdapter =
    ACC.Authority.Infrastructure.Adapters.AccountingSubject.RecognizedAccountingSubjectAdapter;
using ChartRecognizedAccountingSubjectAdapter =
    ACC.ChartOfAccounts.Infrastructure.Adapters.AccountingSubject.RecognizedAccountingSubjectAdapter;
using ChartOfAccountsAggregate = ACC.ChartOfAccounts.Domain.Aggregates.ChartOfAccounts;

namespace ACC.Application.Tests.TestKit;

internal sealed class ApplicationUseCaseTestContext
{
    private readonly InMemoryAccountingSubjectStore accountingSubjects = new();
    private readonly InMemoryRoleAssignmentStore roleAssignments = new();
    private readonly InMemoryChartOfAccountsStore charts = new();
    private readonly InMemoryAccountStore accounts = new();
    private readonly TestRecognizedUserPort recognizedUsers = new();
    private readonly TestChartOfAccountsTemplateCatalog templates = new();

    public ApplicationUseCaseTestContext()
    {
        var eventStore = new InMemoryEventStore();
        var roleAssignmentRepository = new EventSourcedRepository<RoleAssignment>(
            eventStore,
            RoleAssignment.Rehydrate);
        var chartRepository = new EventSourcedRepository<ChartOfAccountsAggregate>(
            eventStore,
            ChartOfAccountsAggregate.Rehydrate);

        var establishInitialOwner = new EstablishInitialOwnerHandler(
            roleAssignmentRepository,
            roleAssignments,
            new RoleAssignmentProjection(roleAssignments),
            recognizedUsers,
            new AuthorityRecognizedAccountingSubjectAdapter(accountingSubjects));

        var authorityPolicy = new AuthorityPolicy(roleAssignments, new RolePowerPolicy());
        var adoptChartOfAccounts = new AdoptChartOfAccountsHandler(
            chartRepository,
            charts,
            templates,
            new ChartRecognizedAccountingSubjectAdapter(accountingSubjects),
            new ChartOfAccountsAuthorityAdapter(authorityPolicy),
            new ChartOfAccountsProjection(charts, accounts));

        CompleteOnboarding = new CompleteOnboardingHandler(
            new CreateAccountingSubjectHandler(accountingSubjects),
            establishInitialOwner,
            adoptChartOfAccounts);
    }

    public CompleteOnboardingHandler CompleteOnboarding { get; }

    public void RecognizeUser(Guid userId) =>
        recognizedUsers.Recognize(userId);

    public void AddTemplate(string id, string name) =>
        templates.Add(new ChartOfAccountsTemplate(
            id,
            name,
            [
                new TemplateAccount("1000", "Assets"),
                new TemplateAccount("2000", "Equity and liabilities")
            ]));

    public AccountingSubjectView? FindAccountingSubject(Guid accountingSubjectId) =>
        accountingSubjects.Find(accountingSubjectId);

    public IReadOnlyCollection<RoleAssignmentView> FindActiveRoles(Guid userId) =>
        roleAssignments.FindActiveByUserId(userId);

    public ChartOfAccountsView? FindChart(Guid accountingSubjectId) =>
        charts.FindFor(accountingSubjectId);

    public IReadOnlyCollection<AccountView> FindAccounts(Guid chartOfAccountsId) =>
        accounts.FindFor(chartOfAccountsId);

}
