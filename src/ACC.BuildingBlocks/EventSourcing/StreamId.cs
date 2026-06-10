namespace ACC.BuildingBlocks.EventSourcing;

public sealed record StreamId(string Value)
{
    public static StreamId For(string streamName, object id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamName);
        ArgumentNullException.ThrowIfNull(id);

        return new StreamId($"{streamName}-{id}");
    }

    public override string ToString() => Value;
}
