using ACC.BuildingBlocks.Domain;

namespace ACC.Authority.Domain.Invariants;

public static class UserMustBeRecognizedForAuthority
{
    public static void Ensure(bool isRecognized, Guid userId)
    {
        if (!isRecognized)
        {
            throw new UserMustBeRecognizedForAuthorityViolation(userId);
        }
    }
}

public sealed class UserMustBeRecognizedForAuthorityViolation(Guid userId)
    : InvariantViolationException(
        $"User {userId} must be recognized before authority can be assigned.");
