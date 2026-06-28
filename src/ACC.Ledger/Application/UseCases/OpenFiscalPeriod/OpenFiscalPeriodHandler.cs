using ACC.BuildingBlocks.EventSourcing;
using ACC.Ledger.Application.Ports.Authority;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Domain.Events;
using ACC.Ledger.Domain.Invariants;
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

        ActorMustHaveLedgerPower.Ensure(
            authority.CanOpenFiscalPeriod(command.ActorUserId, command.AccountingSubjectId),
            command.ActorUserId,
            "open a fiscal period");

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
