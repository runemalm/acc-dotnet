using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.ChartOfAccounts.Application.Ports.Authority;
using ACC.ChartOfAccounts.Domain.Events;
using ACC.ChartOfAccounts.Domain.Invariants;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;

public sealed class DeactivateAccountHandler
{
    private readonly EventSourcedRepository<Domain.Aggregates.ChartOfAccounts> charts;
    private readonly IChartOfAccountsAuthorityPort authority;
    private readonly ChartOfAccountsProjection projection;

    public DeactivateAccountHandler(
        EventSourcedRepository<Domain.Aggregates.ChartOfAccounts> charts,
        IChartOfAccountsAuthorityPort authority,
        ChartOfAccountsProjection projection)
    {
        this.charts = charts;
        this.authority = authority;
        this.projection = projection;
    }

    public DeactivateAccountResult Handle(DeactivateAccountCommand command, DateTimeOffset deactivatedAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);
        var chart = LoadChart(command.ChartOfAccountsId);

        ActorMustHaveChartOfAccountsPower.Ensure(
            authority.CanManageChartOfAccounts(command.ActorUserId, chart.AccountingSubjectId),
            command.ActorUserId,
            "deactivate an account");

        chart.DeactivateAccount(command.AccountNumber, command.ActorUserId, deactivatedAt);
        var storedEvents = charts.Save(ChartStream(command.ChartOfAccountsId), chart);
        projection.Project(storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<AccountDeactivated>()
            .Single());

        return new DeactivateAccountResult(command.AccountNumber);
    }

    private Domain.Aggregates.ChartOfAccounts LoadChart(Guid chartOfAccountsId)
        => charts.Find(ChartStream(chartOfAccountsId))
           ?? throw new RequiredObjectNotFoundException(
               $"Chart of accounts {chartOfAccountsId} could not be found.");

    private static void ValidateCommand(DeactivateAccountCommand command)
    {
        if (command.ActorUserId == Guid.Empty || command.ChartOfAccountsId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Deactivating an account must identify the acting user and chart of accounts.");
        }

        if (string.IsNullOrWhiteSpace(command.AccountNumber))
        {
            throw new ApplicationValidationException(
                "An account must have a number.");
        }
    }

    private static StreamId ChartStream(Guid chartOfAccountsId) =>
        StreamId.For("chart-of-accounts", chartOfAccountsId);
}
