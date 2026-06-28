using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.ChartOfAccounts.Application.Ports.Authority;
using ACC.ChartOfAccounts.Domain.Events;
using ACC.ChartOfAccounts.Domain.Invariants;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Application.UseCases.AddAccount;

public sealed class AddAccountHandler
{
    private readonly EventSourcedRepository<Domain.Aggregates.ChartOfAccounts> charts;
    private readonly IChartOfAccountsAuthorityPort authority;
    private readonly ChartOfAccountsProjection projection;

    public AddAccountHandler(
        EventSourcedRepository<Domain.Aggregates.ChartOfAccounts> charts,
        IChartOfAccountsAuthorityPort authority,
        ChartOfAccountsProjection projection)
    {
        this.charts = charts;
        this.authority = authority;
        this.projection = projection;
    }

    public AddAccountResult Handle(AddAccountCommand command, DateTimeOffset addedAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        var chart = LoadChart(command.ChartOfAccountsId);

        ActorMustHaveChartOfAccountsPower.Ensure(
            authority.CanManageChartOfAccounts(command.ActorUserId, chart.AccountingSubjectId),
            command.ActorUserId,
            "add an account");

        chart.AddAccount(command.AccountNumber, command.AccountName, command.ActorUserId, addedAt);
        var storedEvents = charts.Save(ChartStream(command.ChartOfAccountsId), chart);
        projection.Project(storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<AccountAdded>()
            .Single());

        return new AddAccountResult(command.AccountNumber);
    }

    private Domain.Aggregates.ChartOfAccounts LoadChart(Guid chartOfAccountsId)
        => charts.Find(ChartStream(chartOfAccountsId))
           ?? throw new ResourceNotFoundException(
               $"Chart of accounts {chartOfAccountsId} could not be found.");

    private static StreamId ChartStream(Guid chartOfAccountsId) =>
        StreamId.For("chart-of-accounts", chartOfAccountsId);
}
