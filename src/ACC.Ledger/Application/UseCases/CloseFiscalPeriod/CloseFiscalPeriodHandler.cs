using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.Ledger.Application.Ports.Authority;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Domain.Events;
using ACC.Ledger.Domain.Invariants;
using ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;

namespace ACC.Ledger.Application.UseCases.CloseFiscalPeriod;

public sealed class CloseFiscalPeriodHandler
{
    private readonly EventSourcedRepository<FiscalPeriod> fiscalPeriods;
    private readonly FiscalPeriodProjection fiscalPeriodProjection;
    private readonly ILedgerAuthorityPort authority;

    public CloseFiscalPeriodHandler(
        EventSourcedRepository<FiscalPeriod> fiscalPeriods,
        FiscalPeriodProjection fiscalPeriodProjection,
        ILedgerAuthorityPort authority)
    {
        this.fiscalPeriods = fiscalPeriods;
        this.fiscalPeriodProjection = fiscalPeriodProjection;
        this.authority = authority;
    }

    public CloseFiscalPeriodResult Handle(CloseFiscalPeriodCommand command, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(command);

        var streamId = FiscalPeriodStream(command.FiscalPeriodId);
        var fiscalPeriod = fiscalPeriods.Load(streamId);

        if (fiscalPeriod.Id == Guid.Empty)
        {
            throw new ResourceNotFoundException(
                $"Fiscal period {command.FiscalPeriodId} could not be found.");
        }

        ActorMustHaveLedgerPower.Ensure(
            authority.CanCloseFiscalPeriod(command.ActorUserId, fiscalPeriod.AccountingSubjectId),
            command.ActorUserId,
            "close a fiscal period");

        fiscalPeriod.Close(occurredAt);
        var storedEvents = fiscalPeriods.Save(streamId, fiscalPeriod);

        var domainEvent = storedEvents
            .Select(storedEvent => storedEvent.Data)
            .OfType<FiscalPeriodClosed>()
            .Single();

        fiscalPeriodProjection.Project(domainEvent);

        return new CloseFiscalPeriodResult(fiscalPeriod.Id);
    }

    private static StreamId FiscalPeriodStream(Guid fiscalPeriodId) =>
        StreamId.For("fiscal-period", fiscalPeriodId);
}
