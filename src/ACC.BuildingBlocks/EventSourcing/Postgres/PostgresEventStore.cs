namespace ACC.BuildingBlocks.EventSourcing.Postgres;

public sealed class PostgresEventStore : IEventStore
{
    public IReadOnlyList<StoredEvent> LoadStream(StreamId streamId) =>
        throw new NotImplementedException("Postgres event storage has not been implemented yet.");

    public IReadOnlyList<StoredEvent> AppendToStream(
        StreamId streamId,
        long expectedVersion,
        IReadOnlyCollection<object> events) =>
        throw new NotImplementedException("Postgres event storage has not been implemented yet.");
}
