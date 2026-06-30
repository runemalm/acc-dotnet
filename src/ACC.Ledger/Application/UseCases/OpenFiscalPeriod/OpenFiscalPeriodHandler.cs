using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.Ledger.Application.Ports.Authority;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Domain.Events;
using ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;

namespace ACC.Ledger.Application.UseCases.OpenFiscalPeriod;

public sealed class OpenFiscalPeriodHandler
{
    private readonly EventSourcedRepository<FiscalPeriod> fiscalPeriods;
    private readonly FiscalPeriodProjection fiscalPeriodProjection;
    private readonly ILedgerAuthorityPort authority;

    public OpenFiscalPeriodHandler(
        EventSourcedRepository<FiscalPeriod> fiscalPeriods,
        FiscalPeriodProjection fiscalPeriodProjection,
        ILedgerAuthorityPort authority)
    {
        this.fiscalPeriods = fiscalPeriods;
        this.fiscalPeriodProjection = fiscalPeriodProjection;
        this.authority = authority;
    }

    public OpenFiscalPeriodResult Handle(OpenFiscalPeriodCommand command, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        if (!authority.CanOpenFiscalPeriod(command.ActorUserId, command.AccountingSubjectId))
        {
            throw new AuthorizationDeniedException(
                $"User {command.ActorUserId} must have power to open a fiscal period.");
        }

        var fiscalPeriodId = Guid.NewGuid();
        var fiscalPeriod = FiscalPeriod.Open(
            fiscalPeriodId,
            command.AccountingSubjectId,
            command.StartsOn,
            command.EndsOn,
            occurredAt);

        var storedEvents = fiscalPeriods.Save(FiscalPeriodStream(fiscalPeriodId), fiscalPeriod);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<FiscalPeriodOpened>()
            .Single();

        fiscalPeriodProjection.Project(domainEvent);

        return new OpenFiscalPeriodResult(fiscalPeriod.Id);
    }

    private static void ValidateCommand(OpenFiscalPeriodCommand command)
    {
        if (command.ActorUserId == Guid.Empty || command.AccountingSubjectId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "Opening a fiscal period must identify the acting user and accounting subject.");
        }

        if (command.EndsOn < command.StartsOn)
        {
            throw new ApplicationValidationException(
                "A fiscal period cannot end before it starts.");
        }
    }

    private static StreamId FiscalPeriodStream(Guid fiscalPeriodId) =>
        StreamId.For("fiscal-period", fiscalPeriodId);
}
