namespace ACC.BuildingBlocks.EventSourcing;

public abstract class EventSourcedAggregate
{
    private readonly List<object> uncommittedEvents = [];

    public long Version { get; private set; }

    public IReadOnlyCollection<object> UncommittedEvents => uncommittedEvents.ToArray();

    public void LoadFromHistory(IEnumerable<object> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        foreach (var domainEvent in events)
        {
            Apply(domainEvent);
            Version++;
        }
    }

    public IReadOnlyCollection<object> DequeueUncommittedEvents()
    {
        var events = uncommittedEvents.ToArray();
        uncommittedEvents.Clear();

        return events;
    }

    protected void Raise(object domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        Apply(domainEvent);
        Version++;
        uncommittedEvents.Add(domainEvent);
    }

    protected abstract void Apply(object domainEvent);
}
