using ACC.BuildingBlocks.Failures;
using ACC.Identity.Domain.Aggregates;

namespace ACC.Identity.Domain.Invariants;

public static class EmailMustNotAlreadyBeVerified
{
    public static void Ensure(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.EmailVerifiedAt is not null)
        {
            throw new StateConflictException(
                "The user's email address has already been verified.");
        }
    }
}
