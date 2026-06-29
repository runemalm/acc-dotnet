using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.ChartOfAccounts.Application.Ports.Authority;
using ACC.ChartOfAccounts.Domain.Events;
using ACC.ChartOfAccounts.Domain.Invariants;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;

namespace ACC.ChartOfAccounts.Application.UseCases.ReactivateAccount;

public sealed class ReactivateAccountHandler
{
    private readonly EventSourcedRepository<Domain.Aggregates.ChartOfAccounts> charts;
    private readonly IChartOfAccountsAuthorityPort authority;
    private readonly ChartOfAccountsProjection projection;

    public ReactivateAccountHandler(
        EventSourcedRepository<Domain.Aggregates.ChartOfAccounts> charts,
        IChartOfAccountsAuthorityPort authority,
        ChartOfAccountsProjection projection)
    {
        this.charts = charts;
        this.authority = authority;
        this.projection = projection;
    }

    public ReactivateAccountResult Handle(ReactivateAccountCommand command, DateTimeOffset reactivatedAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);
        var chart = LoadChart(command.ChartOfAccountsId);

        ActorMustHaveChartOfAccountsPower.Ensure(
            authority.CanManageChartOfAccounts(command.ActorUserId, chart.AccountingSubjectId),
            command.ActorUserId,
            "reactivate an account");

        chart.ReactivateAccount(command.AccountNumber, command.ActorUserId, reactivatedAt);
        var storedEvents = charts.Save(ChartStream(command.ChartOfAccountsId), chart);
        projection.Project(storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<AccountReactivated>()
            .Single());

        return new ReactivateAccountResult(command.AccountNumber);
    }

    private Domain.Aggregates.ChartOfAccounts LoadChart(Guid chartOfAccountsId)
        => charts.Find(ChartStream(chartOfAccountsId))
           ?? throw new RequiredObjectNotFoundException(
               $"Chart of accounts {chartOfAccountsId} could not be found.");

    private static void ValidateCommand(ReactivateAccountCommand command)
    {
        if (command.ActorUserId == Guid.Empty || command.ChartOfAccountsId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Reactivating an account must identify the acting user and chart of accounts.");
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
