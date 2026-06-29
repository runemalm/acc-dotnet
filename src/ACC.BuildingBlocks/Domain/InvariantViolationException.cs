namespace ACC.BuildingBlocks.Domain;

public abstract class InvariantViolationException : Exception
{
    protected InvariantViolationException(string message)
        : base(message)
    {
    }
}
