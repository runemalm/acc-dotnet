using System.Net.Mail;

namespace ACC.Identity.Domain.Invariants;

public static class UserEmailMustBeValid
{
    public static void Ensure(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("A user email address must be valid.");
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
            throw new InvalidOperationException("A user email address must be valid.");
        }
    }
}
