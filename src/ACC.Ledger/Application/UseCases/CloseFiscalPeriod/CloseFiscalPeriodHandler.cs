using ACC.BuildingBlocks.EventSourcing;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Domain.Events;
using ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;

namespace ACC.Ledger.Application.UseCases.CloseFiscalPeriod;

public sealed class CloseFiscalPeriodHandler
{
    private readonly EventSourcedRepository<FiscalPeriod> fiscalPeriods;
    private readonly FiscalPeriodProjection fiscalPeriodProjection;

    public CloseFiscalPeriodHandler(
        EventSourcedRepository<FiscalPeriod> fiscalPeriods,
        FiscalPeriodProjection fiscalPeriodProjection)
    {
        this.fiscalPeriods = fiscalPeriods;
        this.fiscalPeriodProjection = fiscalPeriodProjection;
    }

    public CloseFiscalPeriodResult Handle(CloseFiscalPeriodCommand command, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(command);

        var streamId = FiscalPeriodStream(command.FiscalPeriodId);
        var fiscalPeriod = fiscalPeriods.Load(streamId);

        if (fiscalPeriod.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Fiscal period {command.FiscalPeriodId} could not be found.");
        }

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
