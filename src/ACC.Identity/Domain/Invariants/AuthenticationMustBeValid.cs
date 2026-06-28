using ACC.BuildingBlocks.Failures;

namespace ACC.Identity.Domain.Invariants;

public static class AuthenticationMustBeValid
{
    public static void Ensure(bool isValid)
    {
        if (!isValid)
        {
            throw new AuthenticationFailedException("Authentication must be valid.");
        }
    }
}
