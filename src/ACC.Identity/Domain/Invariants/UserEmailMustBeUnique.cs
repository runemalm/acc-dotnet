using ACC.BuildingBlocks.Domain;

namespace ACC.Identity.Domain.Invariants;

public static class UserEmailMustBeUnique
{
    public static void Ensure(bool isAvailable, string email)
    {
        if (!isAvailable)
        {
            throw new UserEmailMustBeUniqueViolation(email);
        }
    }
}

public sealed class UserEmailMustBeUniqueViolation(string email)
    : InvariantViolationException($"A user with email address {email} already exists.");
