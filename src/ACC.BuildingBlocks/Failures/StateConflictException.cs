namespace ACC.BuildingBlocks.Failures;

public class StateConflictException : InvalidOperationException
{
    public StateConflictException(string message)
        : base(message)
    {
    }
}
