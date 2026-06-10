using ACC.BuildingBlocks.EventSourcing;
using ACC.Ledger.Domain.Events;
using ACC.Ledger.Domain.Invariants;

namespace ACC.Ledger.Domain.Aggregates;

public sealed class FiscalPeriod : EventSourcedAggregate
{
    private FiscalPeriod()
    {
    }

    private static void EnsureCanOpen(
        Guid id,
        Guid accountingSubjectId,
        DateOnly startsOn,
        DateOnly endsOn)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A fiscal period must have an identity.", nameof(id));
        }

        if (accountingSubjectId == Guid.Empty)
        {
            throw new ArgumentException(
                "A fiscal period must belong to an accounting subject.",
                nameof(accountingSubjectId));
        }

        if (endsOn < startsOn)
        {
            throw new ArgumentException("A fiscal period cannot end before it starts.", nameof(endsOn));
        }
    }

    public Guid Id { get; private set; }

    public Guid AccountingSubjectId { get; private set; }

    public DateOnly StartsOn { get; private set; }

    public DateOnly EndsOn { get; private set; }

    public FiscalPeriodStatus Status { get; private set; }

    public bool IsOpen => Status == FiscalPeriodStatus.Open;

    public bool Contains(DateOnly date) => StartsOn <= date && date <= EndsOn;

    public void Close(DateTimeOffset occurredAt)
    {
        FiscalPeriodMustBeOpenToClose.Ensure(this);

        Raise(new FiscalPeriodClosed(Id, AccountingSubjectId, occurredAt));
    }

    public static FiscalPeriod Open(
        Guid id,
        Guid accountingSubjectId,
        DateOnly startsOn,
        DateOnly endsOn,
        DateTimeOffset occurredAt)
    {
        EnsureCanOpen(id, accountingSubjectId, startsOn, endsOn);

        var fiscalPeriod = new FiscalPeriod();
        fiscalPeriod.Raise(new FiscalPeriodOpened(
            id,
            accountingSubjectId,
            startsOn,
            endsOn,
            occurredAt));

        return fiscalPeriod;
    }

    public static FiscalPeriod Rehydrate(IEnumerable<object> events)
    {
        var fiscalPeriod = new FiscalPeriod();
        fiscalPeriod.LoadFromHistory(events);

        return fiscalPeriod;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case FiscalPeriodOpened opened:
                Apply(opened);
                break;
            case FiscalPeriodClosed closed:
                Apply(closed);
                break;
        }
    }

    private void Apply(FiscalPeriodOpened domainEvent)
    {
        Id = domainEvent.FiscalPeriodId;
        AccountingSubjectId = domainEvent.AccountingSubjectId;
        StartsOn = domainEvent.StartsOn;
        EndsOn = domainEvent.EndsOn;
        Status = FiscalPeriodStatus.Open;
    }

    private void Apply(FiscalPeriodClosed domainEvent)
    {
        Status = FiscalPeriodStatus.Closed;
    }
}

public enum FiscalPeriodStatus
{
    Open,
    Closed
}
