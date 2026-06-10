namespace ACC.Identity.Domain.Invariants;

public static class UserEmailMustBeUnique
{
    public static void Ensure(bool isAvailable, string email)
    {
        if (!isAvailable)
        {
            throw new InvalidOperationException(
                $"A user with email address {email} already exists.");
        }
    }
}
