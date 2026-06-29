using System.Net.Mail;
using ACC.BuildingBlocks.Domain;

namespace ACC.Identity.Domain.Invariants;

public static class UserEmailMustBeValid
{
    public static void Ensure(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UserEmailMustBeValidViolation();
        }

        try
        {
            var address = new MailAddress(email);

            if (email != email.Trim() || address.Address != email)
            {
                throw new FormatException();
            }
        }
        catch (FormatException)
        {
            throw new UserEmailMustBeValidViolation();
        }
    }
}

public sealed class UserEmailMustBeValidViolation()
    : InvariantViolationException("A user email address must be valid.");
