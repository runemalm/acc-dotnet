using ACC.BuildingBlocks.Failures;

namespace ACC.BuildingBlocks.EventSourcing;

public sealed class WrongExpectedStreamVersionException : StateConflictException
{
    public WrongExpectedStreamVersionException(StreamId streamId, long expectedVersion, long actualVersion)
        : base($"Stream '{streamId}' was expected to be at version {expectedVersion}, but was at version {actualVersion}.")
    {
        StreamId = streamId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public StreamId StreamId { get; }

    public long ExpectedVersion { get; }

    public long ActualVersion { get; }
}
