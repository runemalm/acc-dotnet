namespace ACC.Identity.Application.Ports.Security;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
