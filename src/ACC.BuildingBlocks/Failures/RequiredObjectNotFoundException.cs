namespace ACC.BuildingBlocks.Failures;

public sealed class RequiredObjectNotFoundException : Exception
{
    public RequiredObjectNotFoundException(string message)
        : base(message)
    {
    }
}
