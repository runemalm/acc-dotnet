using ACC.Identity.Domain.Aggregates;

namespace ACC.Identity.Domain.Invariants;

public static class EmailVerificationMustBeValid
{
    public static void Ensure(User user, string token, DateTimeOffset verifiedAt)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(token) ||
            user.EmailVerificationToken != token ||
            user.EmailVerificationTokenExpiresAt is null ||
            user.EmailVerificationTokenExpiresAt <= verifiedAt)
        {
            throw new InvalidOperationException("Email verification must be valid.");
        }
    }
}
