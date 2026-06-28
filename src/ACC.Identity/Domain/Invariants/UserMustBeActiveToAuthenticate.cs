using ACC.BuildingBlocks.Failures;
using ACC.Identity.Domain.Aggregates;

namespace ACC.Identity.Domain.Invariants;

public static class UserMustBeActiveToAuthenticate
{
    public static void Ensure(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!user.IsActive)
        {
            throw new AuthenticationFailedException("Authentication must be valid.");
        }
    }
}
