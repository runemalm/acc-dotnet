using ACC.BuildingBlocks.EventSourcing;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Domain.Events;
using ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;

namespace ACC.Ledger.Application.UseCases.OpenFiscalPeriod;

public sealed class OpenFiscalPeriodHandler
{
    private readonly EventSourcedRepository<FiscalPeriod> fiscalPeriods;
    private readonly FiscalPeriodProjection fiscalPeriodProjection;

    public OpenFiscalPeriodHandler(
        EventSourcedRepository<FiscalPeriod> fiscalPeriods,
        FiscalPeriodProjection fiscalPeriodProjection)
    {
        this.fiscalPeriods = fiscalPeriods;
        this.fiscalPeriodProjection = fiscalPeriodProjection;
    }

    public OpenFiscalPeriodResult Handle(OpenFiscalPeriodCommand command, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(command);

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

    private static StreamId FiscalPeriodStream(Guid fiscalPeriodId) =>
        StreamId.For("fiscal-period", fiscalPeriodId);
}
