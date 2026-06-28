namespace ACC.BuildingBlocks.Failures;

public sealed class ResourceNotFoundException : InvalidOperationException
{
    public ResourceNotFoundException(string message)
        : base(message)
    {
    }
}
