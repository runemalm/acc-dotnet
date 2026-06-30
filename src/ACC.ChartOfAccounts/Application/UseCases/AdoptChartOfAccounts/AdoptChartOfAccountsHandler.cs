using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.ChartOfAccounts.Application.Ports.AccountingSubject;
using ACC.ChartOfAccounts.Application.Ports.Authority;
using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Application.Ports.Templates;
using ACC.ChartOfAccounts.Domain.Aggregates;
using ACC.ChartOfAccounts.Domain.Events;
using ACC.ChartOfAccounts.Domain.Invariants;
using ACC.ChartOfAccounts.Domain.Templates;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;

public sealed class AdoptChartOfAccountsHandler
{
    private readonly EventSourcedRepository<Domain.Aggregates.ChartOfAccounts> charts;
    private readonly IChartOfAccountsStore chartStore;
    private readonly IChartOfAccountsTemplateCatalog templates;
    private readonly IRecognizedAccountingSubjectPort recognizedAccountingSubjects;
    private readonly IChartOfAccountsAuthorityPort authority;
    private readonly ChartOfAccountsProjection projection;

    public AdoptChartOfAccountsHandler(
        EventSourcedRepository<Domain.Aggregates.ChartOfAccounts> charts,
        IChartOfAccountsStore chartStore,
        IChartOfAccountsTemplateCatalog templates,
        IRecognizedAccountingSubjectPort recognizedAccountingSubjects,
        IChartOfAccountsAuthorityPort authority,
        ChartOfAccountsProjection projection)
    {
        this.charts = charts;
        this.chartStore = chartStore;
        this.templates = templates;
        this.recognizedAccountingSubjects = recognizedAccountingSubjects;
        this.authority = authority;
        this.projection = projection;
    }

    public AdoptChartOfAccountsResult Handle(
        AdoptChartOfAccountsCommand command,
        DateTimeOffset adoptedAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        if (!recognizedAccountingSubjects.IsRecognizedAccountingSubject(command.AccountingSubjectId))
        {
            throw new RequiredObjectNotFoundException(
                $"Accounting subject {command.AccountingSubjectId} is required to adopt a chart of accounts.");
        }

        AccountingSubjectMustHaveAtMostOneOperativeChartOfAccounts.Ensure(
            chartStore.FindFor(command.AccountingSubjectId) is null,
            command.AccountingSubjectId);

        if (!authority.CanAdoptChartOfAccounts(command.ActorUserId, command.AccountingSubjectId))
        {
            throw new AuthorizationDeniedException(
                $"User {command.ActorUserId} must have power to adopt a chart of accounts.");
        }

        var template = templates.Find(command.TemplateId)
            ?? throw new RequiredObjectNotFoundException(
                $"Chart of accounts template {command.TemplateId} is not recognized.");

        var chartOfAccountsId = Guid.NewGuid();
        var chart = Domain.Aggregates.ChartOfAccounts.Adopt(
            chartOfAccountsId,
            command.AccountingSubjectId,
            new AdoptedChartOfAccountsTemplate(
                template.Id,
                template.Name),
            template.Accounts
                .Select(account => new Account(account.Number, account.Name))
                .ToArray(),
            command.ActorUserId,
            adoptedAt);

        var storedEvents = charts.Save(ChartStream(chartOfAccountsId), chart);
        projection.Project(storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<ChartOfAccountsAdopted>()
            .Single());

        return new AdoptChartOfAccountsResult(chartOfAccountsId);
    }

    private static StreamId ChartStream(Guid chartOfAccountsId) =>
        StreamId.For("chart-of-accounts", chartOfAccountsId);

    private static void ValidateCommand(AdoptChartOfAccountsCommand command)
    {
        if (command.ActorUserId == Guid.Empty || command.AccountingSubjectId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Adopting a chart of accounts must identify the acting user and accounting subject.");
        }

        if (string.IsNullOrWhiteSpace(command.TemplateId))
        {
            throw new ApplicationValidationException(
                "A chart of accounts template must be identified.");
        }
    }
}
