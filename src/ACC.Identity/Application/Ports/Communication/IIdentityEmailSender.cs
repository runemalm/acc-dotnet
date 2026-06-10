namespace ACC.Identity.Application.Ports.Communication;

public interface IIdentityEmailSender
{
    void SendVerificationEmail(
        string email,
        string verificationToken,
        DateTimeOffset expiresAt);
}
