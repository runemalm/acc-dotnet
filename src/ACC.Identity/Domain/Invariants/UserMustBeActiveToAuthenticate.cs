using ACC.BuildingBlocks.Domain;
using ACC.Identity.Domain.Aggregates;

namespace ACC.Identity.Domain.Invariants;

public static class UserMustBeActiveToAuthenticate
{
    public static void Ensure(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!user.IsActive)
        {
            throw new UserMustBeActiveToAuthenticateViolation();
        }
    }
}

public sealed class UserMustBeActiveToAuthenticateViolation()
    : InvariantViolationException("Authentication must be valid.");
