using ACC.Identity.Domain.Aggregates;
using ACC.BuildingBlocks.Domain;

namespace ACC.Identity.Domain.Invariants;

public static class EmailVerificationMustBeValid
{
    public static void EnsureRecognized(bool isRecognized)
    {
        if (!isRecognized)
        {
            throw new EmailVerificationMustBeValidViolation();
        }
    }

    public static void Ensure(User user, string token, DateTimeOffset verifiedAt)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(token) ||
            user.EmailVerificationToken != token ||
            user.EmailVerificationTokenExpiresAt is null ||
            user.EmailVerificationTokenExpiresAt <= verifiedAt)
        {
            throw new EmailVerificationMustBeValidViolation();
        }
    }
}

public sealed class EmailVerificationMustBeValidViolation()
    : InvariantViolationException("Email verification must be valid.");
