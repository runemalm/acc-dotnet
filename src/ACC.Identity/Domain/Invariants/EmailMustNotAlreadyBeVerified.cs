using ACC.Identity.Domain.Aggregates;
using ACC.BuildingBlocks.Domain;

namespace ACC.Identity.Domain.Invariants;

public static class EmailMustNotAlreadyBeVerified
{
    public static void Ensure(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.EmailVerifiedAt is not null)
        {
            throw new EmailMustNotAlreadyBeVerifiedViolation();
        }
    }
}

public sealed class EmailMustNotAlreadyBeVerifiedViolation()
    : InvariantViolationException("The user's email address has already been verified.");
