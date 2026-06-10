namespace ACC.BuildingBlocks.EventSourcing;

public sealed record StoredEvent(
    Guid EventId,
    StreamId StreamId,
    long StreamVersion,
    string EventType,
    object Data,
    DateTimeOffset RecordedAt);
