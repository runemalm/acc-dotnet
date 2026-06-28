namespace ACC.BuildingBlocks.Failures;

public sealed class SemanticViolationException : InvalidOperationException
{
    public SemanticViolationException(string message)
        : base(message)
    {
    }
}
