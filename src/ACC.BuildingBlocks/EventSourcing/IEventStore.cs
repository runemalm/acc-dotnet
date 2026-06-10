namespace ACC.BuildingBlocks.EventSourcing;

public interface IEventStore
{
    IReadOnlyList<StoredEvent> LoadStream(StreamId streamId);

    IReadOnlyList<StoredEvent> AppendToStream(
        StreamId streamId,
        long expectedVersion,
        IReadOnlyCollection<object> events);
}
