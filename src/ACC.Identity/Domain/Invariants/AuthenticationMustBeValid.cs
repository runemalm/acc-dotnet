namespace ACC.Identity.Domain.Invariants;

public static class AuthenticationMustBeValid
{
    public static void Ensure(bool isValid)
    {
        if (!isValid)
        {
            throw new InvalidOperationException("Authentication must be valid.");
        }
    }
}
