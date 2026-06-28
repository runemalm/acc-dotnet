namespace ACC.BuildingBlocks.Authorization;

public sealed class AuthorizationDeniedException : Exception
{
    public AuthorizationDeniedException(string message)
        : base(message)
    {
    }
}
