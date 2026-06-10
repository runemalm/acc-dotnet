using System.Collections.Concurrent;
namespace ACC.BuildingBlocks.EventSourcing.Memory;

public sealed class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<StreamId, List<StoredEvent>> streams = new();
    private readonly object gate = new();

    public IReadOnlyList<StoredEvent> LoadStream(StreamId streamId)
    {
        ArgumentNullException.ThrowIfNull(streamId);

        lock (gate)
        {
            return streams.TryGetValue(streamId, out var events)
                ? events.ToArray()
                : Array.Empty<StoredEvent>();
        }
    }

    public IReadOnlyList<StoredEvent> AppendToStream(
        StreamId streamId,
        long expectedVersion,
        IReadOnlyCollection<object> events)
    {
        ArgumentNullException.ThrowIfNull(streamId);
        ArgumentNullException.ThrowIfNull(events);

        lock (gate)
        {
            var stream = streams.GetOrAdd(streamId, _ => []);
            var actualVersion = stream.Count;

            if (actualVersion != expectedVersion)
            {
                throw new WrongExpectedStreamVersionException(streamId, expectedVersion, actualVersion);
            }

            var storedEvents = events
                .Select((domainEvent, index) => new StoredEvent(
                    Guid.NewGuid(),
                    streamId,
                    actualVersion + index + 1,
                    domainEvent.GetType().Name,
                    domainEvent,
                    DateTimeOffset.UtcNow))
                .ToArray();

            stream.AddRange(storedEvents);

            return storedEvents;
        }
    }
}
