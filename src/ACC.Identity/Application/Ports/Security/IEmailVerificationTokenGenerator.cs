namespace ACC.Identity.Application.Ports.Security;

public interface IEmailVerificationTokenGenerator
{
    string Generate();
}
