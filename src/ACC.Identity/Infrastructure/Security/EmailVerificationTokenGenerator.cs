using System.Security.Cryptography;
using ACC.Identity.Application.Ports.Security;

namespace ACC.Identity.Infrastructure.Security;

public sealed class EmailVerificationTokenGenerator : IEmailVerificationTokenGenerator
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
