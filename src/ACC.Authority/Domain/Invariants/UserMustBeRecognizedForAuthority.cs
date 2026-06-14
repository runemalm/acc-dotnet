namespace ACC.Authority.Domain.Invariants;

public static class UserMustBeRecognizedForAuthority
{
    public static void Ensure(bool isRecognized, Guid userId)
    {
        if (!isRecognized)
        {
            throw new InvalidOperationException(
                $"User {userId} must be recognized before authority can be assigned.");
        }
    }
}
