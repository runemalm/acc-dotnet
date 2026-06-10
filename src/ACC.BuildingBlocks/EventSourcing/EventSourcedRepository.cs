namespace ACC.BuildingBlocks.EventSourcing;

public sealed class EventSourcedRepository<TAggregate>
    where TAggregate : EventSourcedAggregate
{
    private readonly IEventStore eventStore;
    private readonly Func<IEnumerable<object>, TAggregate> rehydrate;

    public EventSourcedRepository(
        IEventStore eventStore,
        Func<IEnumerable<object>, TAggregate> rehydrate)
    {
        this.eventStore = eventStore;
        this.rehydrate = rehydrate;
    }

    public TAggregate Load(StreamId streamId)
    {
        ArgumentNullException.ThrowIfNull(streamId);

        var events = eventStore
            .LoadStream(streamId)
            .Select(storedEvent => storedEvent.Data);

        return rehydrate(events);
    }

    public IReadOnlyList<StoredEvent> Save(StreamId streamId, TAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(streamId);
        ArgumentNullException.ThrowIfNull(aggregate);

        var expectedVersion = aggregate.Version - aggregate.UncommittedEvents.Count;
        var domainEvents = aggregate.DequeueUncommittedEvents();

        return eventStore.AppendToStream(streamId, expectedVersion, domainEvents);
    }
}
