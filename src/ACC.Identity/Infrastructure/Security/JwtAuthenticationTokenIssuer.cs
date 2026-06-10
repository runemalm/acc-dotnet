using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ACC.Identity.Application.Ports.Security;
using Microsoft.Extensions.Configuration;

namespace ACC.Identity.Infrastructure.Security;

public sealed class JwtAuthenticationTokenIssuer : IAuthenticationTokenIssuer
{
    private readonly string issuer;
    private readonly string audience;
    private readonly string signingKey;
    private readonly int expiresMinutes;

    public JwtAuthenticationTokenIssuer(IConfiguration configuration)
    {
        issuer = configuration["Identity:Jwt:Issuer"] ?? "ACC";
        audience = configuration["Identity:Jwt:Audience"] ?? "ACC";
        signingKey = configuration["Identity:Jwt:SigningKey"] ?? string.Empty;
        expiresMinutes = int.TryParse(configuration["Identity:Jwt:ExpiresMinutes"], out var configured)
            ? configured
            : 60;
    }

    public AuthenticationToken Issue(Guid userId, string email, DateTimeOffset issuedAt)
    {
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var expiresAt = issuedAt.AddMinutes(expiresMinutes);
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["email"] = email,
            ["iss"] = issuer,
            ["aud"] = audience,
            ["iat"] = ToUnixTimeSeconds(issuedAt),
            ["exp"] = ToUnixTimeSeconds(expiresAt)
        };

        var headerSegment = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var payloadSegment = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{headerSegment}.{payloadSegment}";
        var signature = Sign(unsignedToken);

        return new AuthenticationToken(
            $"{unsignedToken}.{signature}",
            expiresAt);
    }

    private string Sign(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));

        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static long ToUnixTimeSeconds(DateTimeOffset value) =>
        value.ToUnixTimeSeconds();

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
