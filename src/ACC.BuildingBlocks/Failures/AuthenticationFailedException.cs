namespace ACC.BuildingBlocks.Failures;

public sealed class AuthenticationFailedException : InvalidOperationException
{
    public AuthenticationFailedException(string message)
        : base(message)
    {
    }
}
