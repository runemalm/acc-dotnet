using ACC.Identity.Application.Ports.Communication;

namespace ACC.Identity.Infrastructure.Communication;

public sealed class ConsoleIdentityEmailSender : IIdentityEmailSender
{
    public void SendVerificationEmail(
        string email,
        string verificationToken,
        DateTimeOffset expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(verificationToken);

        Console.WriteLine("=== VERIFICATION EMAIL ===");
        Console.WriteLine($"To: {email}");
        Console.WriteLine($"Verification token: {verificationToken}");
        Console.WriteLine("==========================");
    }
}
